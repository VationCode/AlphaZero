using alpha.installer;
using UnityEngine;

namespace alpha.ingame
{
    public class InGameManager : MonoBehaviour
    {
        [SerializeField]
        private PlayerInstaller m_playerInstaller;
        [SerializeField]
        private CameraInstaller m_cameraInstaller;

        private void Awake()
        {
            m_playerInstaller.Install();
            m_cameraInstaller.Install();
        }

        void Start()
        {

        }
    }
}