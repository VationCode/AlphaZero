using alpha.camera;
using alpha.input;
using UnityEngine;

namespace alpha.installer
{
    public class CameraInstaller : MonoBehaviour
    {
        [SerializeField]
        private CameraCore m_cameraCore;
        [SerializeField]
        private InputSystemBoundary m_inputSystemBoundary;
        public void Install()
        {
            m_cameraCore.Bind(m_inputSystemBoundary);
        }
    }
}