using AntGame.Core.Animation;
using AntGame.Characters.Scout;
using UnityEngine;

namespace AntGame.Characters.Scout.Animation
{
    public sealed class ScoutAnimation : CharacterAnimationBase
    {
        private ScoutController controller;

        private static readonly int IsMovingHash =
            Animator.StringToHash("IsMoving");

        private static readonly int IsActionHash =
            Animator.StringToHash("IsAction");

        private static readonly int DamageHash =
            Animator.StringToHash("Damage");

        private static readonly int DeathHash =
            Animator.StringToHash("Death");

        protected override void Awake()
        {
            base.Awake();
            controller = GetComponentInParent<ScoutController>();
        }

        private void OnEnable()
        {
            if (controller == null)
                controller = GetComponentInParent<ScoutController>();

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
            Animator.SetBool(
                IsMovingHash,
                state == CharacterAnimationState.Move
            );

            Animator.SetBool(
                IsActionHash,
                state == CharacterAnimationState.Action
            );

            switch (state)
            {
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