using UnityEngine;
using Islebound.Core;

namespace Islebound.Weather
{
    public enum WeatherType
    {
        Clear,
        Rain,
        Wind,
        Fog
    }

    public class WeatherManager : MonoBehaviour
    {
        public static WeatherManager Instance { get; private set; }

        [Header("Weather Cycle")]
        [SerializeField] private bool autoCycleWeather = true;
        [SerializeField] private float weatherDurationInSeconds = 75f;
        [SerializeField] private WeatherType currentWeather = WeatherType.Clear;

        private float weatherTimer;
        private WeatherType lastBroadcastWeather;

        public WeatherType CurrentWeather => currentWeather;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            lastBroadcastWeather = currentWeather;
            BroadcastWeather();
        }

        private void Update()
        {
            // Поддержка изменения значения прямо из Inspector во время Play Mode
            if (lastBroadcastWeather != currentWeather)
            {
                lastBroadcastWeather = currentWeather;
                BroadcastWeather();
            }

            if (!autoCycleWeather || weatherDurationInSeconds <= 0f)
                return;

            weatherTimer += Time.deltaTime;
            if (weatherTimer >= weatherDurationInSeconds)
            {
                weatherTimer = 0f;
                SetWeather(GetNextWeather(currentWeather));
            }
        }

        public void SetWeather(WeatherType weatherType)
        {
            if (currentWeather == weatherType)
                return;

            currentWeather = weatherType;
            lastBroadcastWeather = currentWeather;
            BroadcastWeather();
        }

        public void NextWeather()
        {
            SetWeather(GetNextWeather(currentWeather));
        }

        private WeatherType GetNextWeather(WeatherType weather)
        {
            return weather switch
            {
                WeatherType.Clear => WeatherType.Rain,
                WeatherType.Rain => WeatherType.Wind,
                WeatherType.Wind => WeatherType.Fog,
                _ => WeatherType.Clear
            };
        }

        private void BroadcastWeather()
        {
            Debug.Log($"Weather changed to: {currentWeather}");
            GameEvents.OnWeatherChanged?.Invoke();
            GameEvents.OnWeatherChangedDetailed?.Invoke(currentWeather);
        }
    }
}