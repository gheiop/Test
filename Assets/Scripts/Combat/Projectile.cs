using System;
using UnityEngine;
using Islebound.Player;

namespace Islebound.Combat
{
    public enum ProjectileResult
    {
        HitPlayer,
        HitObstacle,
        Expired
    }

    public class Projectile : MonoBehaviour
    {
        [Header("Flight")]
        [SerializeField] private float speed = 12f;
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float hitRadius = 0.2f;

        [Header("Collision")]
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private bool ignoreOtherProjectiles = true;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Debug")]
        [SerializeField] private bool debugLogs = false;

        private Vector3 direction;
        private GameObject owner;
        private bool isFinished;

        public Action<ProjectileResult, Vector3> OnProjectileFinished;

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
            isFinished = false;
        }

        private void Update()
        {
            if (isFinished)
                return;

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
                    HandleHit(hit.collider, hit.point);
                    return;
                }
            }

            transform.position = nextPosition;

            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Finish(ProjectileResult.Expired, transform.position);
            }
        }

        private void HandleHit(Collider other, Vector3 hitPoint)
        {
            if (other == null)
            {
                Finish(ProjectileResult.HitObstacle, transform.position);
                return;
            }

            if (owner != null && other.transform.root.gameObject == owner.transform.root.gameObject)
            {
                return;
            }

            if (ignoreOtherProjectiles && other.GetComponentInParent<Projectile>() != null)
            {
                return;
            }

            DamageablePlayer player = other.GetComponentInParent<DamageablePlayer>();
            if (player != null)
            {
                if (debugLogs)
                {
                    Debug.Log($"[Projectile] Hit player for {damage}");
                }

                player.TakeDamage(damage);
                Finish(ProjectileResult.HitPlayer, hitPoint);
                return;
            }

            if (!other.isTrigger)
            {
                if (debugLogs)
                {
                    Debug.Log($"[Projectile] Hit obstacle: {other.name}");
                }

                Finish(ProjectileResult.HitObstacle, hitPoint);
            }
        }

        private void Finish(ProjectileResult result, Vector3 point)
        {
            if (isFinished)
                return;

            isFinished = true;
            OnProjectileFinished?.Invoke(result, point);
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, hitRadius);
        }
    }
}