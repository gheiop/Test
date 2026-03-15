using System.Collections.Generic;
using UnityEngine;

namespace Islebound.Factions
{
    public class FactionManager : MonoBehaviour
    {
        public static FactionManager Instance { get; private set; }

        [Header("Registered Factions")]
        [SerializeField] private FactionData[] factionDefinitions;

        private readonly Dictionary<FactionType, int> reputationMap = new();
        private readonly Dictionary<FactionType, FactionData> factionDataMap = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildFactionLookup();
        }

        private void BuildFactionLookup()
        {
            reputationMap.Clear();
            factionDataMap.Clear();

            foreach (FactionData faction in factionDefinitions)
            {
                if (faction == null)
                    continue;

                factionDataMap[faction.factionType] = faction;
                reputationMap[faction.factionType] = faction.defaultReputation;
            }
        }

        public int GetReputation(FactionType factionType)
        {
            return reputationMap.TryGetValue(factionType, out int value) ? value : 0;
        }

        public void AddReputation(FactionType factionType, int amount)
        {
            int current = GetReputation(factionType);
            reputationMap[factionType] = Mathf.Clamp(current + amount, -100, 100);
            Debug.Log($"Faction {factionType} reputation: {reputationMap[factionType]}");
        }

        public void SetReputation(FactionType factionType, int value)
        {
            reputationMap[factionType] = Mathf.Clamp(value, -100, 100);
            Debug.Log($"Faction {factionType} reputation set to: {reputationMap[factionType]}");
        }

        public bool IsHostile(FactionType factionType)
        {
            FactionData data = GetFactionData(factionType);
            int reputation = GetReputation(factionType);

            if (data == null)
                return reputation < 0;

            return reputation <= data.hostileThreshold;
        }

        public bool IsFriendly(FactionType factionType)
        {
            FactionData data = GetFactionData(factionType);
            int reputation = GetReputation(factionType);

            if (data == null)
                return reputation > 0;

            return reputation >= data.friendlyThreshold;
        }

        public FactionData GetFactionData(FactionType factionType)
        {
            factionDataMap.TryGetValue(factionType, out FactionData data);
            return data;
        }
    }
}