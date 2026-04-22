using alpha.camera;
using alpha.player;
using UnityEngine;
using static UnityEngine.InputManagerEntry;

public class InGameManager : MonoBehaviour
{
    [SerializeField]
    private PlayerCore m_playerCore;
    [SerializeField]
    private CameraCore m_cameraCore;


    private void Awake()
    {
        
    }
    void Start()
    {
        m_cameraCore.Bind(m_playerCore.m_inputBoundary);
    }

}
