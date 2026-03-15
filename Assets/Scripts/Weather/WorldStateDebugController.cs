using UnityEngine;
using Islebound.World;

namespace Islebound.Weather
{
    public class WorldStateDebugController : MonoBehaviour
    {
        [Header("Hotkeys")]
        [SerializeField] private KeyCode nextSeasonKey = KeyCode.F7;
        [SerializeField] private KeyCode nextWeatherKey = KeyCode.F8;
        [SerializeField] private KeyCode morningKey = KeyCode.F9;
        [SerializeField] private KeyCode noonKey = KeyCode.F10;
        [SerializeField] private KeyCode nightKey = KeyCode.F11;

        private void Update()
        {
            if (Input.GetKeyDown(nextSeasonKey) && SeasonManager.Instance != null)
                SeasonManager.Instance.NextSeason();

            if (Input.GetKeyDown(nextWeatherKey) && WeatherManager.Instance != null)
                WeatherManager.Instance.NextWeather();

            if (WorldTimeManager.Instance == null)
                return;

            if (Input.GetKeyDown(morningKey))
                WorldTimeManager.Instance.SetNormalizedTime(0.23f);

            if (Input.GetKeyDown(noonKey))
                WorldTimeManager.Instance.SetNormalizedTime(0.5f);

            if (Input.GetKeyDown(nightKey))
                WorldTimeManager.Instance.SetNormalizedTime(0.85f);
        }
    }
}