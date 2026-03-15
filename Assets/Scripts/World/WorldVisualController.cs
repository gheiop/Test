using UnityEngine;
using Islebound.Core;
using Islebound.Weather;

namespace Islebound.World
{
    public class WorldVisualController : MonoBehaviour
    {
        [Header("Global Lighting")]
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private Color dayAmbientColor = new Color(0.75f, 0.8f, 0.9f);
        [SerializeField] private Color nightAmbientColor = new Color(0.12f, 0.14f, 0.22f);

        [Header("Fog")]
        [SerializeField] private bool useFog = true;
        [SerializeField] private Color clearFogColor = new Color(0.75f, 0.82f, 0.9f);
        [SerializeField] private Color rainFogColor = new Color(0.55f, 0.6f, 0.68f);
        [SerializeField] private Color windFogColor = new Color(0.72f, 0.77f, 0.82f);
        [SerializeField] private Color fogWeatherColor = new Color(0.62f, 0.68f, 0.72f);

        [SerializeField] private float clearFogDensity = 0.002f;
        [SerializeField] private float rainFogDensity = 0.006f;
        [SerializeField] private float windFogDensity = 0.0035f;
        [SerializeField] private float fogWeatherDensity = 0.012f;

        [Header("Biomes")]
        [SerializeField] private BiomeVisualGroup[] biomeVisualGroups;

        private void OnEnable()
        {
            GameEvents.OnSeasonChangedDetailed += HandleSeasonChanged;
            GameEvents.OnWeatherChangedDetailed += HandleWeatherChanged;
            GameEvents.OnDayNightStateChanged += HandleDayNightChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnSeasonChangedDetailed -= HandleSeasonChanged;
            GameEvents.OnWeatherChangedDetailed -= HandleWeatherChanged;
            GameEvents.OnDayNightStateChanged -= HandleDayNightChanged;
        }

        private void Start()
        {
            if (SeasonManager.Instance != null)
                HandleSeasonChanged(SeasonManager.Instance.CurrentSeason);

            if (WeatherManager.Instance != null)
                HandleWeatherChanged(WeatherManager.Instance.CurrentWeather);

            if (WorldTimeManager.Instance != null)
                HandleDayNightChanged(WorldTimeManager.Instance.IsNight);
        }

        private void HandleSeasonChanged(SeasonType season)
        {
            foreach (BiomeVisualGroup group in biomeVisualGroups)
            {
                if (group != null)
                    group.ApplySeason(season);
            }
        }

        private void HandleWeatherChanged(WeatherType weather)
        {
            RenderSettings.fog = useFog;

            switch (weather)
            {
                case WeatherType.Rain:
                    RenderSettings.fogColor = rainFogColor;
                    RenderSettings.fogDensity = rainFogDensity;
                    break;
                case WeatherType.Wind:
                    RenderSettings.fogColor = windFogColor;
                    RenderSettings.fogDensity = windFogDensity;
                    break;
                case WeatherType.Fog:
                    RenderSettings.fogColor = fogWeatherColor;
                    RenderSettings.fogDensity = fogWeatherDensity;
                    break;
                default:
                    RenderSettings.fogColor = clearFogColor;
                    RenderSettings.fogDensity = clearFogDensity;
                    break;
            }
        }

        private void HandleDayNightChanged(bool isNight)
        {
            RenderSettings.ambientLight = isNight ? nightAmbientColor : dayAmbientColor;

            if (skyboxMaterial != null)
            {
                if (skyboxMaterial.HasProperty("_Exposure"))
                    skyboxMaterial.SetFloat("_Exposure", isNight ? 0.55f : 1.15f);
            }
        }
    }
}