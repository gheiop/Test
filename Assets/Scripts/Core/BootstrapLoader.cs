using UnityEngine;
using UnityEngine.SceneManagement;

namespace Islebound.Core
{
    public class BootstrapLoader : MonoBehaviour
    {
        [SerializeField] private string targetSceneName = "Prototype";

        private void Start()
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}