using alpha.input;
using UnityEngine;

namespace alpha.camera
{
    [RequireComponent(typeof(CameraMovementModule))]
    public class CameraCore : MonoBehaviour
    {
        private AlphaInputSystem m_inputSystemBoundary;
        [SerializeField]
        private CameraMovementModule m_movementModule;

        private void Awake()
        {
            m_movementModule = GetComponent<CameraMovementModule>();
        }
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        public void Bind(AlphaInputSystem inputSystemBoundary)
        {
            m_inputSystemBoundary = inputSystemBoundary;

            m_movementModule.Bind(m_inputSystemBoundary);
        }
    }
}