using UnityEngine;

namespace Islebound.Player
{
    public class PlayerCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.65f, 0f);
        [SerializeField] private float mouseSensitivity = 0.15f;
        [SerializeField] private float minPitch = -40f;
        [SerializeField] private float maxPitch = 75f;

        private float yaw;
        private float pitch;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Look(Vector2 lookInput)
        {
            yaw += lookInput.x * mouseSensitivity;
            pitch -= lookInput.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.position = target.position + offset;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

            target.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        public void ToggleCursor(bool shown)
        {
            Cursor.lockState = shown ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = shown;
        }
    }
}