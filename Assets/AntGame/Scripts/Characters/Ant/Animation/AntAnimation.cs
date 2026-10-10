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

        private static readonly int IsActionHash =
            Animator.StringToHash("IsAction");

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