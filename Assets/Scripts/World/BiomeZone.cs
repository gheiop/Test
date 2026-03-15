using UnityEngine;

namespace Islebound.World
{
    public enum BiomeType
    {
        Forest,
        MeadowPlains,
        StonePlateau
    }

    [RequireComponent(typeof(Collider))]
    public class BiomeZone : MonoBehaviour
    {
        [SerializeField] private BiomeType biomeType;

        public BiomeType BiomeType => biomeType;

        private Collider cachedCollider;

        private void Awake()
        {
            cachedCollider = GetComponent<Collider>();
        }

        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        public bool ContainsPoint(Vector3 point)
        {
            if (cachedCollider == null)
                cachedCollider = GetComponent<Collider>();

            return cachedCollider.bounds.Contains(point);
        }

        public Bounds GetBounds()
        {
            if (cachedCollider == null)
                cachedCollider = GetComponent<Collider>();

            return cachedCollider.bounds;
        }

        public Vector3 GetRandomPointInsideBounds(float yOffset = 0f)
        {
            Bounds bounds = GetBounds();

            float x = Random.Range(bounds.min.x, bounds.max.x);
            float z = Random.Range(bounds.min.z, bounds.max.z);
            float y = bounds.max.y + yOffset;

            return new Vector3(x, y, z);
        }
    }
}