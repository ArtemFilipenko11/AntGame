using AntGame.Core.Animation;
using AntGame.Characters.Ant;
using UnityEngine;

namespace AntGame.Characters.Ant.Animation
{
    public sealed class AntAnimation : CharacterAnimationBase
    {
        private AntController controller;

        private static readonly int IsMovingHash =
            Animator.StringToHash("IsMoving");

        private static readonly int ActionHash =
            Animator.StringToHash("Action");

        private static readonly int DamageHash =
            Animator.StringToHash("Damage");

        private static readonly int DeathHash =
            Animator.StringToHash("Death");

        protected override void Awake()
        {
            base.Awake();
            controller = GetComponentInParent<AntController>();
        }

        private void OnEnable()
        {
            if (controller == null)
                controller = GetComponentInParent<AntController>();

            if (controller == null)
                return;

            controller.AnimationStateChanged += OnAnimationStateChanged;
            OnAnimationStateChanged(controller.AnimationState);
        }

        private void OnDisable()
        {
            if (controller != null)
                controller.AnimationStateChanged -= OnAnimationStateChanged;
        }

        private void OnAnimationStateChanged(CharacterAnimationState state)
        {
            switch (state)
            {
                case CharacterAnimationState.Idle:
                    Animator.SetBool(IsMovingHash, false);
                    break;

                case CharacterAnimationState.Move:
                    Animator.SetBool(IsMovingHash, true);
                    break;

                case CharacterAnimationState.Action:
                    Animator.SetTrigger(ActionHash);
                    break;

                case CharacterAnimationState.Damage:
                    Animator.SetTrigger(DamageHash);
                    break;

                case CharacterAnimationState.Death:
                    Animator.SetTrigger(DeathHash);
                    break;
            }
        }
    }
}