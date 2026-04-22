using alpha.camera;
using alpha.camera.module;
using alpha.player;
using alpha.player.boundary;
using UnityEngine;

public class PlayerInstaller : MonoBehaviour
{
    [SerializeField]
    private PlayerCore m_playerCore;
    [SerializeField]
    private InputSystemBoundary m_inputSystemBoundary;
    [SerializeField]
    private CameraCore m_cameraCore;

    public void Install()
    {
        m_playerCore.Bind(m_inputSystemBoundary);
    }
}
