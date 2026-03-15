using UnityEngine;

namespace Islebound.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public bool IsGamePaused { get; private set; }

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

        public void SetPause(bool paused)
        {
            IsGamePaused = paused;
            Time.timeScale = paused ? 0f : 1f;
        }
    }
}