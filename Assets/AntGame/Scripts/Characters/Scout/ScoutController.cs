using System;
using UnityEngine;

using AntGame.Core;
using AntGame.Core.Navigation;
using AntGame.Food;
using AntGame.Movement;
using AntGame.Characters.Ant;
using AntGame.Core.Animation;

using AntGame.Characters.Scout.AI;
using AntGame.Characters.Scout.AI.Tasks;
using AntGame.Characters.Scout.AI.Behaviours;

namespace AntGame.Characters.Scout
{
    [RequireComponent(typeof(Movement2D))]
    public sealed class ScoutController : ControllerBase<ScoutController>
    {
        private Movement2D movement;
        private AntFoodSensor foodSensor;

        private ScoutDependencies dependencies;
        private ScoutBrain brain;

        public Vector2 Position => transform.position;

        public Vector2 ExplorationCenter =>
            dependencies != null
                ? dependencies.ExplorationCenter
                : Vector2.zero;

        public Vector2 ExplorationSize =>
            dependencies != null
                ? dependencies.ExplorationSize
                : Vector2.zero;

        public float ExplorationStep =>
            dependencies != null
                ? dependencies.ExplorationStep
                : 0f;

        public Vector2 ExplorationOrigin =>
            dependencies != null
                ? dependencies.ExplorationOrigin
                : Vector2.zero;

        public event Action<MovementEndReason> MovementEnded;

        public event Action<FoodSource> FoodSourceDetected;
        public event Action<ScoutController> Destroyed;
        public CharacterAnimationState AnimationState { get; private set; }
            = CharacterAnimationState.Idle;

        public event Action<CharacterAnimationState> AnimationStateChanged;

        public void SetAnimationState(CharacterAnimationState newState)
        {
            if (AnimationState == newState)
                return;

            AnimationState = newState;
            AnimationStateChanged?.Invoke(AnimationState);
        }

        private void Awake()
        {
            CacheComponents();
        }

        private void CacheComponents()
        {
            if (movement == null)
                movement = GetComponent<Movement2D>();

            if (foodSensor == null)
            {
                foodSensor =
                    GetComponentInChildren<AntFoodSensor>(true);
            }
        }

        public bool Initialize(ScoutDependencies newDependencies)
        {
            if (IsInitialized || newDependencies == null)
                return false;

            CacheComponents();

            if (newDependencies.Navigation == null)
            {
                Debug.LogError(
                    "ScoutController has no navigation system.",
                    this
                );

                return false;
            }

            if (newDependencies.ExplorationSize.x <= 0f ||
                newDependencies.ExplorationSize.y <= 0f ||
                newDependencies.ExplorationStep <= 0f)
            {
                Debug.LogError(
                    "ScoutController has invalid exploration bounds or step.",
                    this
                );

                return false;
            }

            if (movement == null)
            {
                Debug.LogError(
                    "ScoutController has no Movement2D.",
                    this
                );

                return false;
            }

            if (foodSensor == null)
            {
                Debug.LogError(
                    "ScoutController has no AntFoodSensor.",
                    this
                );

                return false;
            }

            if (!foodSensor.Initialize())
                return false;

            if (!movement.Initialize(newDependencies.Navigation))
                return false;

            dependencies = newDependencies;

            brain = new ScoutBrain(this);

            if (!RegisterBehaviour<ScoutExploreTask>(
                    new ScoutExploreBehaviour(this)))
            {
                return false;
            }

            if (!InitializeBehaviours())
                return false;

            if (!brain.Initialize())
                return false;

            movement.MovementStarted += OnMovementStarted;
            movement.MovementEnded += OnMovementEnded;
            foodSensor.FoodDetected += OnFoodDetected;

            if (!InitializeBase())
            {
                movement.MovementEnded -= OnMovementEnded;
                foodSensor.FoodDetected -= OnFoodDetected;

                return false;
            }

            brain.Reevaluate();

            return true;
        }

        public bool MoveToPosition(Vector2 position)
        {
            if (!IsInitialized || movement == null)
                return false;

            return movement.MoveTo(position);
        }

        public void CancelMovement()
        {
            if (movement != null)
                movement.Stop();
        }

        public bool CompleteCurrentTask()
        {
            if (!IsInitialized || brain == null)
                return false;

            return brain.CompleteCurrentTask();
        }

        private void OnMovementEnded(MovementEndReason reason)
        {
            SetAnimationState(CharacterAnimationState.Idle);

            MovementEnded?.Invoke(reason);
        }

        private void OnFoodDetected(FoodSource source)
        {
            FoodSourceDetected?.Invoke(source);
        }

        private void OnMovementStarted()
        {
            SetAnimationState(CharacterAnimationState.Move);
        }

        private void OnDestroy()
        {
            if (movement != null)
                movement.MovementStarted -= OnMovementStarted;
                movement.MovementEnded -= OnMovementEnded;

            if (foodSensor != null)
                foodSensor.FoodDetected -= OnFoodDetected;

            Destroyed?.Invoke(this);
        }
    }
}