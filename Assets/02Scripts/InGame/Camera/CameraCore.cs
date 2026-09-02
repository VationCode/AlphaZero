using alpha.input;
using UnityEngine;

namespace alpha.camera
{
    public class CameraCore : MonoBehaviour
    {
        private AlphaInputSystem _input;

        private CameraMovementModule _movementModule;

        private void Awake()
        {
            _movementModule = GetComponent<CameraMovementModule>();
        }
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            _movementModule.Bind(_input);
        }

        public void Bind(AlphaInputSystem p_input)
        {
            _input = p_input;
        }
    }
}