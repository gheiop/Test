using UnityEngine;

namespace Islebound.Core
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [field: SerializeField] public GameObject GameplayHUD { get; private set; }

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
    }
}