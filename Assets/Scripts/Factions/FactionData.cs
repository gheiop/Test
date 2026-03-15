using UnityEngine;

namespace Islebound.Factions
{
    public enum FactionType
    {
        ForestGuardians,
        Stoneborn,
        WildAnimals
    }

    [CreateAssetMenu(fileName = "FactionData_", menuName = "Islebound/Factions/Faction Data")]
    public class FactionData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName;
        public FactionType factionType;

        [Header("Behavior")]
        [Range(-100, 100)] public int defaultReputation = 0;
        [Range(-100, 100)] public int hostileThreshold = -25;
        [Range(-100, 100)] public int friendlyThreshold = 25;

        [TextArea] public string description;
    }
}