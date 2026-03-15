using UnityEngine;
using Islebound.Core;

namespace Islebound.World
{
    public class PlayerBiomeDetector : MonoBehaviour
    {
        public static PlayerBiomeDetector Instance { get; private set; }

        [Header("Detection")]
        [SerializeField] private Transform detectionPoint;
        [SerializeField] private float detectionRadius = 0.75f;
        [SerializeField] private LayerMask biomeLayerMask = ~0;
        [SerializeField] private float checkInterval = 0.15f;
        [SerializeField] private bool drawDebugGizmos = true;

        private float checkTimer;
        private BiomeZone currentBiomeZone;

        public BiomeZone CurrentBiomeZone => currentBiomeZone;
        public BiomeType CurrentBiomeType => currentBiomeZone != null ? currentBiomeZone.BiomeType : BiomeType.MeadowPlains;

        private void Awake()
        {
            Instance = this;
        }

        private void Reset()
        {
            detectionPoint = transform;
        }

        private void Update()
        {
            checkTimer += Time.deltaTime;
            if (checkTimer < checkInterval)
                return;

            checkTimer = 0f;
            DetectBiome();
        }

        private void DetectBiome()
        {
            Vector3 point = detectionPoint != null ? detectionPoint.position : transform.position;

            Collider[] hits = Physics.OverlapSphere(
                point,
                detectionRadius,
                biomeLayerMask,
                QueryTriggerInteraction.Collide);

            BiomeZone foundBiome = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                BiomeZone biomeZone = hits[i].GetComponent<BiomeZone>();
                if (biomeZone == null)
                    continue;

                Vector3 closestPoint = hits[i].ClosestPoint(point);
                float sqrDistance = (closestPoint - point).sqrMagnitude;

                if (sqrDistance < closestDistance)
                {
                    closestDistance = sqrDistance;
                    foundBiome = biomeZone;
                }
            }

            if (foundBiome == currentBiomeZone)
                return;

            currentBiomeZone = foundBiome;

            if (currentBiomeZone != null)
            {
                GameEvents.OnPlayerBiomeChanged?.Invoke(currentBiomeZone.BiomeType);
                Debug.Log($"Current biome: {currentBiomeZone.BiomeType}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawDebugGizmos)
                return;

            Vector3 point = detectionPoint != null ? detectionPoint.position : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(point, detectionRadius);
        }
    }
}