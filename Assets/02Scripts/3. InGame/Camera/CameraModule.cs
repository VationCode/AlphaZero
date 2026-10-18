using alpha.ingame.input;
using UnityEngine;

public class CameraModule : MonoBehaviour
{
    [SerializeField] private Transform _rig;
    [SerializeField] private Transform _pivot;
    [SerializeField] private Transform _shoulder;
    [SerializeField] private Transform _zoom;
    [SerializeField] private Transform _shake;
    [SerializeField] private Transform _target;

    [Header("[Settings]")]
    [SerializeField] private float _vertical = 2.5f;
    [SerializeField] private float _horizontal = 0f;
    [SerializeField] private float _followSmoothTime = 0.12f;

    [Header("Rotation")]
    [SerializeField] private float _limitAngleY = 60f;
    [SerializeField] private float _sensitivityX = 15f;
    [SerializeField] private float _sensitivityY = 10f;

    [Header("Zoom")]
    [SerializeField] private float _zoomMin = 1f;
    [SerializeField] private float _zoomMax = 4.5f;
    [SerializeField] private float _sensitivityZoom = 1f;
    [SerializeField] private float _zoomSmoothSpeed = 10f;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField, Min(0.01f)] private float _collisionRadius = 0.4f;
    [SerializeField, Min(0f)] private float _collisionPadding = 0.1f;

    public Camera Cam { get; private set; }

    private Vector3 _followVelocity;
    private float _mouseX;
    private float _mouseY;
    private float _currentZoom; // 휠로 정한 희망 거리

    private void Awake()
    {
        Cam = Camera.main;
    }

    private void Start()
    {
        SettingsCamera(_vertical, _horizontal, _zoomMax);
    }

    public void SettingsCamera(float p_vertical, float p_horizontal, float p_distance)
    {
        _pivot.localPosition = new Vector3(0f, p_vertical, 0f);
        _shoulder.localPosition = new Vector3(p_horizontal, 0f, 0f);
        _zoom.localPosition = new Vector3(0f, 0f, -p_distance);

        _currentZoom = p_distance;
    }

    private void FollowTarget()
    {
        if (_rig == null || _target == null) return;
        _rig.position = Vector3.SmoothDamp(_rig.position, _target.position, ref _followVelocity, _followSmoothTime);
    }

    private void Rotation(Vector2 p_lookInput)
    {
        _mouseX += p_lookInput.x * _sensitivityX * 0.01f;
        _mouseY = Mathf.Clamp(_mouseY - (p_lookInput.y * _sensitivityY * 0.01f), -_limitAngleY, _limitAngleY);

        _pivot.localRotation = Quaternion.Euler(_mouseY, _mouseX, 0f);
    }

    private void Zoom(float p_wheel)
    {
        // 휠 입력으로 원하는 거리 변경
        _currentZoom = Mathf.Clamp(_currentZoom - (p_wheel * _sensitivityZoom), _zoomMin, _zoomMax);

        float next = Mathf.Lerp(-_zoom.localPosition.z, _currentZoom, Time.deltaTime * _zoomSmoothSpeed);

        // 현재 위치와 희망 위치 사이의 장애물을 검사
        float castDistance = Mathf.Max(-_zoom.localPosition.z, _currentZoom);
        Vector3 dir = _shoulder.position - _shoulder.forward * castDistance;
        if (Physics.Linecast(_shoulder.position, dir, out RaycastHit hit, _obstacleMask, QueryTriggerInteraction.Ignore))
        {
            next = Mathf.Min(next, Mathf.Max(0f, hit.distance - _collisionPadding));
        }

        Vector3 local = _zoom.localPosition;
        local.z = -next;
        _zoom.localPosition = local;
    }

    private void LateUpdate()
    {
        FollowTarget();
        Rotation(AlphaInput.Instance.LookInput);
        Zoom(AlphaInput.Instance.ZoomInput.y);
    }
}
