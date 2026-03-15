using System.Collections.Generic;
using UnityEngine;
using Islebound.Weather;

namespace Islebound.World
{
    public class BiomeVisualGroup : MonoBehaviour
    {
        [SerializeField] private BiomeType biomeType;
        [SerializeField] private List<Renderer> biomeRenderers = new();

        [Header("Season Colors")]
        [SerializeField] private Color springColor = new Color(0.65f, 1f, 0.65f);
        [SerializeField] private Color summerColor = new Color(0.45f, 0.9f, 0.45f);
        [SerializeField] private Color autumnColor = new Color(0.95f, 0.55f, 0.2f);
        [SerializeField] private Color winterColor = new Color(0.85f, 0.9f, 1f);

        [Header("Biome Tint")]
        [SerializeField] private Color biomeBaseTint = Color.white;

        public BiomeType BiomeType => biomeType;

        public void ApplySeason(SeasonType season)
        {
            Color seasonColor = season switch
            {
                SeasonType.Spring => springColor,
                SeasonType.Summer => summerColor,
                SeasonType.Autumn => autumnColor,
                SeasonType.Winter => winterColor,
                _ => Color.white
            };

            Color finalColor = biomeBaseTint * seasonColor;

            foreach (Renderer rend in biomeRenderers)
            {
                if (rend == null || rend.material == null)
                    continue;

                if (rend.material.HasProperty("_Color"))
                    rend.material.color = finalColor;
            }
        }
    }
}