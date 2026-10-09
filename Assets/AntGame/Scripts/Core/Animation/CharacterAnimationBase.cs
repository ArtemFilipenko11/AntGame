using UnityEngine;

namespace AntGame.Core.Animation
{
    [RequireComponent(typeof(Animator))]
    public abstract class CharacterAnimationBase : MonoBehaviour
    {
        protected Animator Animator { get; private set; }

        protected virtual void Awake()
        {
            Animator = GetComponent<Animator>();
        }

        protected void SetBool(string parameterName, bool value)
        {
            if (Animator == null)
                return;

            Animator.SetBool(parameterName, value);
        }

        protected void SetTrigger(string parameterName)
        {
            if (Animator == null)
                return;

            Animator.SetTrigger(parameterName);
        }

        protected void ResetTrigger(string parameterName)
        {
            if (Animator == null)
                return;

            Animator.ResetTrigger(parameterName);
        }
    }
}