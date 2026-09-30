using alpha.scene;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public void NextScene()
    {
        SceneLoader.LoadInGameScene();
    }
}
