using TMPro;
using UnityEngine;
using Islebound.Factions;

namespace Islebound.Combat
{
    public class CombatDebugUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text debugText;

        private void Update()
        {
            if (debugText == null || FactionManager.Instance == null)
                return;

            int forestRep = FactionManager.Instance.GetReputation(FactionType.ForestGuardians);
            int stoneRep = FactionManager.Instance.GetReputation(FactionType.Stoneborn);
            int wildRep = FactionManager.Instance.GetReputation(FactionType.WildAnimals);

            debugText.text =
                $"Forest Guardians: {forestRep}\n" +
                $"Stoneborn: {stoneRep}\n" +
                $"Wild Animals: {wildRep}";
        }
    }
}