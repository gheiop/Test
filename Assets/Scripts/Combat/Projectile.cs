using UnityEngine;
using Islebound.Player;

namespace Islebound.Combat
{
    public class Projectile : MonoBehaviour
    {
        [Header("Flight")]
        [SerializeField] private float speed = 12f;
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float hitRadius = 0.2f;

        [Header("Collision")]
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private bool destroyOnWorldHit = true;
        [SerializeField] private bool ignoreOtherProjectiles = true;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Debug")]
        [SerializeField] private bool debugLogs = false;

        private Vector3 direction;
        private GameObject owner;

        public void Initialize(
            Vector3 dir,
            float projectileSpeed,
            float projectileLifetime,
            int projectileDamage,
            GameObject projectileOwner,
            float projectileHitRadius)
        {
            direction = dir.normalized;
            speed = projectileSpeed;
            lifetime = projectileLifetime;
            damage = projectileDamage;
            owner = projectileOwner;
            hitRadius = projectileHitRadius;
        }

        private void Update()
        {
            Vector3 currentPosition = transform.position;
            Vector3 nextPosition = currentPosition + direction * speed * Time.deltaTime;
            Vector3 travel = nextPosition - currentPosition;
            float distance = travel.magnitude;

            if (distance > 0f)
            {
                if (Physics.SphereCast(
                    currentPosition,
                    hitRadius,
                    travel.normalized,
                    out RaycastHit hit,
                    distance,
                    hitMask,
                    triggerInteraction))
                {
                    bool consumed = HandleHit(hit.collider);

                    if (consumed)
                    {
                        return;
                    }
                }
            }

            transform.position = nextPosition;

            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private bool HandleHit(Collider other)
        {
            if (other == null)
            {
                Destroy(gameObject);
                return true;
            }

            if (owner != null && other.transform.root.gameObject == owner.transform.root.gameObject)
            {
                return false;
            }

            if (ignoreOtherProjectiles && other.GetComponentInParent<Projectile>() != null)
            {
                if (debugLogs)
                {
                    Debug.Log($"[Projectile] Ignored other projectile: {other.name}");
                }

                return false;
            }

            DamageablePlayer player = other.GetComponentInParent<DamageablePlayer>();
            if (player != null)
            {
                if (debugLogs)
                {
                    Debug.Log($"[Projectile] Hit player for {damage}");
                }

                player.TakeDamage(damage);
                Destroy(gameObject);
                return true;
            }

            if (destroyOnWorldHit && !other.isTrigger)
            {
                if (debugLogs)
                {
                    Debug.Log($"[Projectile] Hit world object: {other.name}");
                }

                Destroy(gameObject);
                return true;
            }

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, hitRadius);
        }
    }
}