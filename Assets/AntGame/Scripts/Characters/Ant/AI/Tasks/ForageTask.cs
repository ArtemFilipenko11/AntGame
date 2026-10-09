using AntGame.Core;

namespace AntGame.Characters.Ant.AI.Tasks
{
    public enum ForageStage
    {
        FindFood,
        MoveToFood,
        CollectFood,
        ReturnToNest,
        DepositFood,
        Finished
    }

    public sealed class ForageTask : TaskBase
    {
        public ForageStage Stage { get; private set; }

        public ForageTask(int priority)
            : base(priority)
        {
            Stage = ForageStage.FindFood;
        }

        public void SetStage(ForageStage stage)
        {
            Stage = stage;
        }
    }
}
