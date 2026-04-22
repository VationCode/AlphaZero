using alpha.camera;
using alpha.player.boundary;
using UnityEngine;
using static UnityEngine.InputManagerEntry;

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
