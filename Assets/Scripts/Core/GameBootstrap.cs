using UnityEngine;
using Islebound.Items;
using Islebound.Factions;

namespace Islebound.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private GameManager gameManagerPrefab;
        [SerializeField] private UIManager uiManagerPrefab;
        [SerializeField] private Islebound.Weather.WeatherManager weatherManagerPrefab;
        [SerializeField] private Islebound.Weather.SeasonManager seasonManagerPrefab;
        [SerializeField] private EventManager eventManagerPrefab;
        [SerializeField] private FactionManager factionManagerPrefab;
        [SerializeField] private InventoryManager inventoryManagerPrefab;

        private void Awake()
        {
            CreateIfMissing(gameManagerPrefab);
            CreateIfMissing(uiManagerPrefab);
            CreateIfMissing(weatherManagerPrefab);
            CreateIfMissing(seasonManagerPrefab);
            CreateIfMissing(eventManagerPrefab);
            CreateIfMissing(factionManagerPrefab);
            CreateIfMissing(inventoryManagerPrefab);
        }

        private void CreateIfMissing<T>(T prefab) where T : MonoBehaviour
        {
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab for {typeof(T).Name} is not assigned in GameBootstrap.");
                return;
            }

            T existing = FindFirstObjectByType<T>();
            if (existing == null)
            {
                Instantiate(prefab);
            }
        }
    }
}