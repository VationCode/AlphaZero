using alpha.ingame.input;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject QuickSlot;
    public bool IsQuickSlotOpen => QuickSlot != null && QuickSlot.activeSelf;

    private CursorLockMode _previousLockState;
    private bool _previousCursorVisible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (QuickSlot != null) QuickSlot.SetActive(false);
    }

    private void Update()
    {
        AlphaInput input = AlphaInput.Instance;
        if (input != null && input.IsQuickSlotPressed)
            Activate(!IsQuickSlotOpen);
        else if (IsQuickSlotOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Activate(false);
    }

    public void Activate(bool p_isActive)
    {
        if (QuickSlot == null || IsQuickSlotOpen == p_isActive) return;

        if (p_isActive)
        {
            _previousLockState = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // UI 상태를 먼저 입력에 전달해 같은 프레임의 공격과 시점 회전을 막는다.
        AlphaInput.Instance?.SetQuickSlotOpen(p_isActive);
        QuickSlot.SetActive(p_isActive);

        if (!p_isActive)
        {
            Cursor.lockState = _previousLockState;
            Cursor.visible = _previousCursorVisible;
        }
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        if (IsQuickSlotOpen) Activate(false);
        Instance = null;
    }
}
