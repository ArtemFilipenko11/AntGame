using System;
using UnityEngine;
using AntGame.Core;
using AntGame.Core.Navigation;
using AntGame.Food;
using AntGame.Movement;
using AntGame.Characters.Ant.AI;
using AntGame.Characters.Ant.AI.Tasks;
using AntGame.Characters.Ant.AI.Behaviours;
using AntGame.Characters.Ant.Animation;
using AntGame.Core.Animation;

namespace AntGame.Characters.Ant
{
    [RequireComponent(
    typeof(Movement2D),
    typeof(AntCarryComponent)
    )]
    public class AntController : ControllerBase<AntController>
    {
        private Movement2D movement;
        private AntDependencies dependencies;
        private AntAnimation antAnimation;
        private AntBrain brain;
        private FoodSource targetFood;
        private AntCarryComponent carryComponent;
        private AntCarryVisual carryVisual;

        public event Action<MovementEndReason> MovementEnded;

        public FoodSource TargetFood => targetFood;
        public int CarriedFood =>
            carryComponent != null
                ? carryComponent.CarriedAmount
                : 0;

        public bool IsCarryingFood => CarriedFood > 0;
        public AntBrain Brain => brain;

        public CharacterAnimationState AnimationState { get; private set; }
            = CharacterAnimationState.Idle;

        public event Action<CharacterAnimationState> AnimationStateChanged;
        public event Action<int> CarriedFoodChanged;
        public event Action<CarryOperationType, bool> CarryOperationCompleted;

        public void SetAnimationState(CharacterAnimationState newState)
        {
            if (AnimationState == newState)
                return;

            AnimationState = newState;
            AnimationStateChanged?.Invoke(AnimationState);
        }

        private void Awake()
        {
            movement = GetComponent<Movement2D>();
            antAnimation = GetComponentInChildren<AntAnimation>();
            carryComponent = GetComponent<AntCarryComponent>();
            carryVisual = GetComponentInChildren<AntCarryVisual>(true);
        }

        public bool Initialize(AntDependencies newDependencies)
        {
            if (IsInitialized || newDependencies == null)
                return false;

            if (newDependencies.Nest == null ||
                !newDependencies.Nest.IsInitialized)
                return false;

            if (newDependencies.FoodRegistry == null ||
                !newDependencies.FoodRegistry.IsInitialized)
                return false;

            if (newDependencies.Navigation == null)
                return false;
            if (carryComponent == null)
                carryComponent = GetComponent<AntCarryComponent>();

            if (carryVisual == null)
                carryVisual = GetComponentInChildren<AntCarryVisual>(true);

            if (carryComponent == null)
            {
                Debug.LogError(
                    $"AntController on {name} has no AntCarryComponent.",
                    this
                );

                return false;
            }

            if (movement == null)
                movement = GetComponent<Movement2D>();

            if (movement == null)
                return false;

            dependencies = newDependencies;

            if (!movement.Initialize(dependencies.Navigation))
                return false;

            movement.MovementEnded += OnMovementEnded;
            movement.MovementStarted += OnMovementStarted;
            
            brain = new AntBrain(this);

            if (!RegisterBehaviour<ForageTask>(
                    new ForageBehaviour(this, brain)))
                return false;

            if (!InitializeBehaviours())
                return false;

            if (!InitializeBase())
                return false;

            if (!brain.Initialize())
                return false;

            if (!carryComponent.Initialize(newDependencies.Nest))
            {
                Debug.LogError("AntCarryComponent initialization failed.", this);
                return false;
            }
            carryComponent.CarriedAmountChanged += OnCarryAmountChanged;
            carryComponent.OperationStarted -= OnCarryOperationStarted;
            carryComponent.OperationStarted += OnCarryOperationStarted;

            carryComponent.OperationFinished -= OnCarryOperationFinished;
            carryComponent.OperationFinished += OnCarryOperationFinished;

            if (!carryVisual.Initialize(this))
            {
                carryComponent.CarriedAmountChanged -= OnCarryAmountChanged;
                carryComponent.OperationStarted -= OnCarryOperationStarted;
                carryComponent.OperationStarted -= OnCarryOperationStarted;

                carryComponent.OperationFinished -= OnCarryOperationFinished;
                carryComponent.OperationFinished -= OnCarryOperationFinished;

                return false;
            }

            return true;
        }
        
        public bool StartForaging(int priority = 0)
        {
            if (!IsInitialized || brain == null)
                return false;

            return brain.StartForaging(priority);
        }

        public FoodSource FindClosestAvailableFood()
        {
            if (!IsInitialized)
                return null;

            return dependencies.FoodRegistry.FindClosestAvailable(
                transform.position);
        }

        public bool SetTargetFood(FoodSource foodSource)
        {
            if (!IsInitialized ||
                foodSource == null ||
                !foodSource.HasFood)
                return false;

            targetFood = foodSource;
            return true;
        }

        public bool MoveToFood()
        {
            if (!IsInitialized ||
                targetFood == null ||
                !targetFood.HasFood)
                return false;

            return movement.MoveTo(targetFood.transform.position);
        }

        public bool MoveToNest()
        {
            if (!IsInitialized || dependencies.Nest == null)
                return false;

            return movement.MoveTo(
                dependencies.Nest.transform.position);
        }

        public void CancelMovement()
        {
            if (movement != null)
                movement.Stop();
        }

        public bool BeginCollectTargetFood()
        {
            if (!IsInitialized ||
                targetFood == null ||
                IsCarryingFood ||
                carryComponent == null)
            {
                return false;
            }

            return carryComponent.BeginCollection(targetFood);
        }

        public bool BeginDepositFood()
        {
            if (!IsInitialized ||
                !IsCarryingFood ||
                carryComponent == null)
            {
                return false;
            }

            return carryComponent.BeginDeposit();
        }

        public void CancelCarryOperation()
        {
            if (carryComponent != null)
                carryComponent.CancelCurrentOperation();
        }

        private void OnMovementStarted()
        {
            SetAnimationState(CharacterAnimationState.Move);
        }

        private void OnMovementEnded(MovementEndReason reason)
        {
            SetAnimationState(CharacterAnimationState.Idle);
            MovementEnded?.Invoke(reason);
        }

        private void OnCarryAmountChanged(int amount)
        {
            CarriedFoodChanged?.Invoke(amount);
        }

        private void OnCarryOperationStarted(CarryOperationType operationType)
        {
            SetAnimationState(CharacterAnimationState.Action);
        }

        private void OnCarryOperationFinished(
            CarryOperationType operationType,
            bool success)
        {
            SetAnimationState(CharacterAnimationState.Idle);

            CarryOperationCompleted?.Invoke(operationType, success);
        }

        private void OnDestroy()
        {
            if (movement != null)
            {
                movement.MovementStarted -= OnMovementStarted;
                movement.MovementEnded -= OnMovementEnded;
            }

            if (carryComponent != null)
            {
                carryComponent.CarriedAmountChanged -= OnCarryAmountChanged;
                carryComponent.OperationStarted -= OnCarryOperationStarted;
                carryComponent.OperationFinished -= OnCarryOperationFinished;
            }
        }
    }
}