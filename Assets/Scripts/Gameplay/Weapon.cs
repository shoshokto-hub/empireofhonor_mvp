using UnityEngine;

namespace EmpireOfHonor.Gameplay
{
    /// <summary>
    /// Simple melee weapon that applies damage within a frontal arc with a cooldown.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private float damage = 15f;
        [SerializeField] private float range = 2f;
        [SerializeField] private float angle = 120f;
        [SerializeField] private float cooldown = 0.8f;
        [SerializeField] private LayerMask hitMask = ~0;

        private float nextAttackTime;
        private Health ownerHealth;

        /// <summary>
        /// Gets the effective attack range of the weapon.
        /// </summary>
        public float Range => range;

        private void Awake()
        {
            ownerHealth = GetComponent<Health>();
            if (attackOrigin == null)
            {
                attackOrigin = transform;
            }
        }

        /// <summary>
        /// Attempts to perform an attack if the cooldown has elapsed.
        /// </summary>
        public bool TryAttack()
        {
            if (Time.time < nextAttackTime)
            {
                return false;
            }

            PerformAttack();
            nextAttackTime = Time.time + cooldown;
            return true;
        }

        private void PerformAttack()
        {
            var origin = attackOrigin != null ? attackOrigin.position : transform.position;
            var forward = attackOrigin != null ? attackOrigin.forward : transform.forward;

            var colliders = Physics.OverlapSphere(origin, range, hitMask, QueryTriggerInteraction.Ignore);
            foreach (var collider in colliders)
            {
                if (collider.attachedRigidbody == null && collider.gameObject == gameObject)
                {
                    continue;
                }

                var health = collider.GetComponentInParent<Health>();
                if (health == null || health == ownerHealth || health.CurrentHealth <= 0f)
                {
                    continue;
                }

                if (health.TeamId == ownerHealth.TeamId)
                {
                    continue;
                }

                var direction = (health.transform.position - origin).normalized;
                var currentAngle = Vector3.Angle(forward, direction);
                if (currentAngle > angle * 0.5f)
                {
                    continue;
                }

                health.ApplyDamage(damage);
            }
        }
    }
}
