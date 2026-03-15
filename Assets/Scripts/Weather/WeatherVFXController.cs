using UnityEngine;
using Islebound.Core;

namespace Islebound.Weather
{
    public class WeatherVFXController : MonoBehaviour
    {
        [SerializeField] private GameObject rainVFX;
        [SerializeField] private GameObject windVFX;
        [SerializeField] private GameObject fogVFX;

        private void OnEnable()
        {
            GameEvents.OnWeatherChangedDetailed += HandleWeatherChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnWeatherChangedDetailed -= HandleWeatherChanged;
        }

        private void Start()
        {
            if (WeatherManager.Instance != null)
                HandleWeatherChanged(WeatherManager.Instance.CurrentWeather);
        }

        private void HandleWeatherChanged(WeatherType weather)
        {
            if (rainVFX != null) rainVFX.SetActive(weather == WeatherType.Rain);
            if (windVFX != null) windVFX.SetActive(weather == WeatherType.Wind);
            if (fogVFX != null) fogVFX.SetActive(weather == WeatherType.Fog);
        }
    }
}