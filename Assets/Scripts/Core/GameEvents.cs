using System;
using Islebound.Items;
using Islebound.Weather;
using Islebound.World;

namespace Islebound.Core
{
    public static class GameEvents
    {
        public static Action OnSeasonChanged;
        public static Action<SeasonType> OnSeasonChangedDetailed;

        public static Action OnWeatherChanged;
        public static Action<WeatherType> OnWeatherChangedDetailed;

        public static Action<float> OnTimeOfDayChanged;
        public static Action<bool> OnDayNightStateChanged;

        public static Action<float, float> OnHealthChanged;
        public static Action<float, float> OnHungerChanged;
        public static Action<float, float> OnWaterChanged;
        public static Action<float, float> OnStaminaChanged;

        public static Action<int> OnHotbarSelectionChanged;
        public static Action<InventorySlot[]> OnInventoryChanged;

        public static Action<string> OnInteractionPromptShown;
        public static Action OnInteractionPromptHidden;

        public static Action<BiomeType> OnPlayerBiomeChanged;
    }
}