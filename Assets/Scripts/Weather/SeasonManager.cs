using UnityEngine;
using Islebound.Core;

namespace Islebound.Weather
{
    public enum SeasonType
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    public class SeasonManager : MonoBehaviour
    {
        public static SeasonManager Instance { get; private set; }

        [Header("Season Cycle")]
        [SerializeField] private bool autoCycleSeasons = true;
        [SerializeField] private float seasonLengthInSeconds = 180f;
        [SerializeField] private SeasonType currentSeason = SeasonType.Summer;

        private float seasonTimer;
        private SeasonType lastBroadcastSeason;

        public SeasonType CurrentSeason => currentSeason;
        public float SeasonLengthInSeconds => seasonLengthInSeconds;

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
            lastBroadcastSeason = currentSeason;
            BroadcastSeason();
        }

        private void Update()
        {
            // Поддержка изменения значения прямо из Inspector во время Play Mode
            if (lastBroadcastSeason != currentSeason)
            {
                lastBroadcastSeason = currentSeason;
                BroadcastSeason();
            }

            if (!autoCycleSeasons || seasonLengthInSeconds <= 0f)
                return;

            seasonTimer += Time.deltaTime;
            if (seasonTimer >= seasonLengthInSeconds)
            {
                seasonTimer = 0f;
                SetSeason(GetNextSeason(currentSeason));
            }
        }

        public void SetSeason(SeasonType seasonType)
        {
            if (currentSeason == seasonType)
                return;

            currentSeason = seasonType;
            lastBroadcastSeason = currentSeason;
            BroadcastSeason();
        }

        public void NextSeason()
        {
            SetSeason(GetNextSeason(currentSeason));
        }

        private SeasonType GetNextSeason(SeasonType season)
        {
            return season switch
            {
                SeasonType.Spring => SeasonType.Summer,
                SeasonType.Summer => SeasonType.Autumn,
                SeasonType.Autumn => SeasonType.Winter,
                _ => SeasonType.Spring
            };
        }

        private void BroadcastSeason()
        {
            Debug.Log($"Season changed to: {currentSeason}");
            GameEvents.OnSeasonChanged?.Invoke();
            GameEvents.OnSeasonChangedDetailed?.Invoke(currentSeason);
        }
    }
}