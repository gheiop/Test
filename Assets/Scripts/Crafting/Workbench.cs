using UnityEngine;
using Islebound.Items;

namespace Islebound.Crafting
{
    [RequireComponent(typeof(Collider))]
    public class Workbench : MonoBehaviour, IInteractable
    {
        [Header("Workbench")]
        [SerializeField] private string stationName = "Workbench";
        [SerializeField] private string interactionText = "[E] Use Workbench";
        [SerializeField] private CraftingRecipeData[] recipes;

        [Header("Debug")]
        [SerializeField] private bool debugLogs = false;

        public string StationName => stationName;
        public CraftingRecipeData[] Recipes => recipes;

        public string GetInteractionText()
        {
            return interactionText;
        }

        public void Interact()
        {
            if (WorkbenchUI.Instance == null)
            {
                Debug.LogWarning("[Workbench] WorkbenchUI.Instance is missing.");
                return;
            }

            if (debugLogs)
            {
                Debug.Log($"[Workbench] Opened station: {stationName}");
            }

            WorkbenchUI.Instance.Open(this);
        }
    }
}