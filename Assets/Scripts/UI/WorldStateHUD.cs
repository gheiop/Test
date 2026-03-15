using TMPro;
using UnityEngine;
using Islebound.Core;
using Islebound.Weather;
using Islebound.World;

namespace Islebound.UI
{
    public class WorldStateHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text worldStateText;

        private BiomeType currentBiome = BiomeType.MeadowPlains;
        private SeasonType currentSeason = SeasonType.Summer;
        private WeatherType currentWeather = WeatherType.Clear;
        private bool isNight;

        private void OnEnable()
        {
            GameEvents.OnPlayerBiomeChanged += HandleBiomeChanged;
            GameEvents.OnSeasonChangedDetailed += HandleSeasonChanged;
            GameEvents.OnWeatherChangedDetailed += HandleWeatherChanged;
            GameEvents.OnDayNightStateChanged += HandleDayNightChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerBiomeChanged -= HandleBiomeChanged;
            GameEvents.OnSeasonChangedDetailed -= HandleSeasonChanged;
            GameEvents.OnWeatherChangedDetailed -= HandleWeatherChanged;
            GameEvents.OnDayNightStateChanged -= HandleDayNightChanged;
        }

        private void Start()
        {
            if (SeasonManager.Instance != null)
                currentSeason = SeasonManager.Instance.CurrentSeason;

            if (WeatherManager.Instance != null)
                currentWeather = WeatherManager.Instance.CurrentWeather;

            if (WorldTimeManager.Instance != null)
                isNight = WorldTimeManager.Instance.IsNight;

            RefreshText();
        }

        private void HandleBiomeChanged(BiomeType biome)
        {
            currentBiome = biome;
            RefreshText();
        }

        private void HandleSeasonChanged(SeasonType season)
        {
            currentSeason = season;
            RefreshText();
        }

        private void HandleWeatherChanged(WeatherType weather)
        {
            currentWeather = weather;
            RefreshText();
        }

        private void HandleDayNightChanged(bool night)
        {
            isNight = night;
            RefreshText();
        }

        private void RefreshText()
        {
            if (worldStateText == null)
                return;

            string timeLabel = isNight ? "Night" : "Day";
            worldStateText.text = $"Biome: {FormatBiome(currentBiome)} | Season: {currentSeason} | Weather: {currentWeather} | Time: {timeLabel}";
        }

        private string FormatBiome(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.Forest => "Forest",
                BiomeType.MeadowPlains => "Meadow Plains",
                BiomeType.StonePlateau => "Stone Plateau",
                _ => biome.ToString()
            };
        }
    }
}