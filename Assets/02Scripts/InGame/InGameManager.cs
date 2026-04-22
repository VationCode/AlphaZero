using alpha.camera;
using alpha.player;
using UnityEngine;
using static UnityEngine.InputManagerEntry;

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
