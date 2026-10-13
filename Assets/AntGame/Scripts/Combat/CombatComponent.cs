using UnityEngine;

namespace AntGame.Combat
{
    public sealed class CombatComponent : MonoBehaviour
    {
        [SerializeField, Min(0f)]
        private float damage = 10f;

        public float Damage => damage;

        public void DealDamage(IDamageable target)
        {
            if (target == null || damage <= 0f)
                return;

            if (target is Object unityObject && unityObject == null)
                return;

            target.TakeDamage(damage);
        }
    }
}
