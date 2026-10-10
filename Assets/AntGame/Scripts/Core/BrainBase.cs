
using System;
using System.Collections.Generic;

namespace AntGame.Core
{
    public abstract class BrainBase<TController>
        where TController : ControllerBase<TController>
    {
        private sealed class TaskEntry
        {
            public TaskBase Task { get; }
            public long Order { get; }

            public TaskEntry(TaskBase task, long order)
            {
                Task = task;
                Order = order;
            }
        }

        protected TController controller;
        private TaskBase defaultTask;
        private readonly List<TaskEntry> pendingTasks = new();
        private readonly List<TaskEntry> suspendedTasks = new();

        private long nextOrder;
        private bool initialized;

        private TaskEntry currentEntry;

        public TaskBase CurrentTask =>
            currentEntry?.Task;

        public bool HasCurrentTask =>
            currentEntry != null;

        public int PendingTaskCount =>
            pendingTasks.Count;

        public int SuspendedTaskCount =>
            suspendedTasks.Count;

        protected BrainBase(TController controller)
        {
            this.controller = controller;
        }

        protected virtual TaskBase CreateDefaultTask()
        {
            return null;
        }

        public virtual bool Initialize()
        {
            if (initialized || controller == null || !controller.IsInitialized)
                return false;

            initialized = true;
            return true;
        }

        public bool SubmitTask(TaskBase task)
        {
            if (!initialized || task == null)
                return false;

            if (ContainsTask(task))
                return false;

            pendingTasks.Add(new TaskEntry(task, nextOrder++));
            Reevaluate();

            return true;
        }

        public void Reevaluate()
        {
            Reevaluate(true);
        }

        private void Reevaluate(bool allowDefaultTask)
        {
            if (!initialized)
                return;

            TaskEntry candidate = FindBestTask();

            if (candidate == null &&
                currentEntry == null &&
                allowDefaultTask)
            {
                EnsureDefaultTask();
                candidate = FindBestTask();
            }

            if (candidate == null)
                return;

            if (currentEntry != null)
            {
                if (candidate.Task.Priority <= currentEntry.Task.Priority)
                    return;

                if (!currentEntry.Task.CanBeInterrupted())
                    return;

                if (!SuspendCurrentTask())
                    return;
            }

            if (TryActivateTask(candidate))
                return;

            if (currentEntry == null)
                TryResumePreviousTask();
        }

        private void EnsureDefaultTask()
        {
            if (defaultTask != null && ContainsTask(defaultTask))
                return;

            defaultTask = CreateDefaultTask();

            if (defaultTask == null)
                return;

            pendingTasks.Add(
                new TaskEntry(defaultTask, nextOrder++)
            );
        }

        public bool CompleteCurrentTask()
        {
            if (currentEntry == null)
                return false;

            TaskEntry completedEntry = currentEntry;

            if (!controller.StopCurrentTask())
                return false;

            currentEntry = null;
            completedEntry.Task.OnComplete();

            Reevaluate();

            return true;
        }

        public bool FailCurrentTask()
        {
            if (currentEntry == null)
                return false;

            TaskEntry failedEntry = currentEntry;

            if (!controller.StopCurrentTask())
                return false;

            currentEntry = null;
            failedEntry.Task.OnFail();

            Reevaluate(
                !ReferenceEquals(failedEntry.Task, defaultTask)
            );

            return true;
        }

        public bool CancelTask(TaskBase task)
        {
            if (task == null)
                return false;

            if (currentEntry != null &&
                ReferenceEquals(currentEntry.Task, task))
            {
                if (!controller.StopCurrentTask())
                    return false;

                currentEntry = null;
                task.OnCancel();

                Reevaluate();

                return true;
            }

            TaskEntry entry = FindEntry(pendingTasks, task);

            if (entry != null)
            {
                pendingTasks.Remove(entry);
                task.OnCancel();
                return true;
            }

            entry = FindEntry(suspendedTasks, task);

            if (entry != null)
            {
                suspendedTasks.Remove(entry);
                task.OnCancel();
                return true;
            }

            return false;
        }

        private bool SuspendCurrentTask()
        {
            if (currentEntry == null)
                return false;

            TaskEntry suspendedEntry = currentEntry;

            if (!controller.SuspendCurrentTask())
                return false;

            currentEntry = null;
            suspendedEntry.Task.OnSuspend();

            suspendedTasks.Add(suspendedEntry);

            return true;
        }

        private bool TryActivateTask(TaskEntry entry)
        {
            bool isSuspended = suspendedTasks.Contains(entry);

            if (isSuspended)
            {
                if (!controller.CanResumeTask(entry.Task))
                    return false;

                if (!controller.ResumeTask(entry.Task))
                    return false;
            }
            else
            {
                if (!controller.CanRunTask(entry.Task))
                    return false;

                if (!controller.StartTask(entry.Task))
                    return false;
            }

            pendingTasks.Remove(entry);
            suspendedTasks.Remove(entry);

            currentEntry = entry;

            if (isSuspended)
                entry.Task.OnResume();
            else
                entry.Task.OnStart();

            return true;
        }

        private void TryResumePreviousTask()
        {
            TaskEntry previousTask = null;

            foreach (TaskEntry entry in suspendedTasks)
            {
                if (!entry.Task.CanResume())
                    continue;

                if (!controller.CanResumeTask(entry.Task))
                    continue;

                if (previousTask == null ||
                    IsBetterCandidate(entry, previousTask))
                {
                    previousTask = entry;
                }
            }

            if (previousTask == null)
                return;

            TryActivateTask(previousTask);
        }



        private TaskEntry FindBestTask()
        {
            TaskEntry bestEntry = null;

            foreach (TaskEntry entry in pendingTasks)
            {
                if (!entry.Task.CanStart())
                    continue;

                if (!controller.CanRunTask(entry.Task))
                    continue;

                if (IsBetterCandidate(entry, bestEntry))
                    bestEntry = entry;
            }

            foreach (TaskEntry entry in suspendedTasks)
            {
                if (!entry.Task.CanResume())
                    continue;

                if (!controller.CanResumeTask(entry.Task))
                    continue;

                if (IsBetterCandidate(entry, bestEntry))
                    bestEntry = entry;
            }

            return bestEntry;
        }

        private static bool IsBetterCandidate(
            TaskEntry candidate,
            TaskEntry currentBest)
        {
            if (currentBest == null)
                return true;

            if (candidate.Task.Priority != currentBest.Task.Priority)
                return candidate.Task.Priority > currentBest.Task.Priority;

            return candidate.Order < currentBest.Order;
        }

        private bool ContainsTask(TaskBase task)
        {
            if (currentEntry != null &&
                ReferenceEquals(currentEntry.Task, task))
                return true;

            return FindEntry(pendingTasks, task) != null ||
                   FindEntry(suspendedTasks, task) != null;
        }

        private static TaskEntry FindEntry(
            List<TaskEntry> entries,
            TaskBase task)
        {
            foreach (TaskEntry entry in entries)
            {
                if (ReferenceEquals(entry.Task, task))
                    return entry;
            }

            return null;
        }
    }
}