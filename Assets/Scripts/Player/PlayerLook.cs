using UnityEngine;

namespace Islebound.Player
{
    public class PlayerLook : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerBody;
        [SerializeField] private Transform cameraPivot;

        [Header("Fallback Settings")]
        [SerializeField] private float mouseSensitivity = 0.12f;
        [SerializeField] private float minPitch = -75f;
        [SerializeField] private float maxPitch = 75f;

        private PlayerDebugSettings debugSettings;
        private float pitch;

        private void Awake()
        {
            debugSettings = GetComponent<PlayerDebugSettings>();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Look(Vector2 lookInput)
        {
            if (IsLookFrozen())
                return;

            float mouseX = lookInput.x * GetMouseSensitivity();
            float mouseY = lookInput.y * GetMouseSensitivity();

            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, GetMinPitch(), GetMaxPitch());

            playerBody.Rotate(Vector3.up * mouseX);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        public void SetCursorVisible(bool visible)
        {
            Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = visible;
        }

        private bool HasLiveDebugSettings()
        {
            return debugSettings != null && debugSettings.UseLiveTuning;
        }

        private bool IsLookFrozen()
        {
            return HasLiveDebugSettings() && debugSettings.FreezeLook;
        }

        private float GetMouseSensitivity() => HasLiveDebugSettings() ? debugSettings.MouseSensitivity : mouseSensitivity;
        private float GetMinPitch() => HasLiveDebugSettings() ? debugSettings.MinPitch : minPitch;
        private float GetMaxPitch() => HasLiveDebugSettings() ? debugSettings.MaxPitch : maxPitch;
    }
}