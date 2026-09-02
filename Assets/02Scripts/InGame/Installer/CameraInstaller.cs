using alpha.camera;
using alpha.input;
using UnityEngine;

namespace alpha.installer
{
    public class CameraInstaller : MonoBehaviour
    {
        [SerializeField]
        private CameraCore _cameraCore;
        [SerializeField]
        private AlphaInputSystem _inputSystem;

        public void Awake()
        {
            _cameraCore.Bind(_inputSystem);
        }
    }
}