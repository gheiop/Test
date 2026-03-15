using UnityEngine;
using Islebound.Core;

namespace Islebound.World
{
    public class WorldTimeManager : MonoBehaviour
    {
        public static WorldTimeManager Instance { get; private set; }

        [Header("Day / Night")]
        [SerializeField] private bool autoCycleTime = true;
        [SerializeField] private float dayLengthInSeconds = 240f;
        [SerializeField] [Range(0f, 1f)] private float normalizedTimeOfDay = 0.25f;

        [Header("Sun")]
        [SerializeField] private Light directionalLight;
        [SerializeField] private Gradient lightColorOverDay;
        [SerializeField] private AnimationCurve lightIntensityOverDay = AnimationCurve.EaseInOut(0f, 0.1f, 1f, 1f);
        [SerializeField] private Vector3 sunRotationOffset = new Vector3(-90f, 170f, 0f);

        public float NormalizedTimeOfDay => normalizedTimeOfDay;
        public bool IsNight => normalizedTimeOfDay < 0.23f || normalizedTimeOfDay > 0.75f;

        private bool lastNightState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            lastNightState = IsNight;
            ApplyTimeVisuals();
            GameEvents.OnTimeOfDayChanged?.Invoke(normalizedTimeOfDay);
            GameEvents.OnDayNightStateChanged?.Invoke(IsNight);
        }

        private void Update()
        {
            if (autoCycleTime && dayLengthInSeconds > 0f)
            {
                normalizedTimeOfDay += Time.deltaTime / dayLengthInSeconds;
                if (normalizedTimeOfDay > 1f)
                    normalizedTimeOfDay -= 1f;
            }

            ApplyTimeVisuals();
            GameEvents.OnTimeOfDayChanged?.Invoke(normalizedTimeOfDay);

            if (lastNightState != IsNight)
            {
                lastNightState = IsNight;
                GameEvents.OnDayNightStateChanged?.Invoke(IsNight);
            }
        }

        public void SetNormalizedTime(float value)
        {
            normalizedTimeOfDay = Mathf.Repeat(value, 1f);
            ApplyTimeVisuals();
            GameEvents.OnTimeOfDayChanged?.Invoke(normalizedTimeOfDay);
        }

        private void ApplyTimeVisuals()
        {
            if (directionalLight == null)
                return;

            float sunAngle = normalizedTimeOfDay * 360f;
            directionalLight.transform.rotation = Quaternion.Euler(sunAngle + sunRotationOffset.x, sunRotationOffset.y, sunRotationOffset.z);
            directionalLight.color = lightColorOverDay.Evaluate(normalizedTimeOfDay);
            directionalLight.intensity = lightIntensityOverDay.Evaluate(normalizedTimeOfDay);
        }
    }
}