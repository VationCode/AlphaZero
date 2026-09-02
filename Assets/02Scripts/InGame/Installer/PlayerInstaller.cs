using alpha.camera;
using alpha.player;
using alpha.input;
using UnityEngine;

namespace alpha.installer
{
    public class PlayerInstaller : MonoBehaviour
    {
        [SerializeField]
        private PlayerCore m_playerCore;
        [SerializeField]
        private AlphaInputSystem m_inputSystemBoundary;
        [SerializeField]
        private CameraCore m_cameraCore;

        public void Install()
        {
            m_playerCore.Bind(m_inputSystemBoundary);
        }
    }
}