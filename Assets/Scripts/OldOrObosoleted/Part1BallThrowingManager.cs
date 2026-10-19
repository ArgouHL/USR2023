using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class Part1BallThrowingManager : MonoBehaviour
{
    [SerializeField]
    private ARRaycastManager _arRaycastManager;
    [SerializeField]
    private ARPlaneManager _arPlaneManager;
    [SerializeField]
    private ARAnchorManager _arAnchorManager;

    [SerializeField]
    private GraphicRaycaster _graphicRaycaster;
    [SerializeField]
    private EventSystem _eventSystem;

    [SerializeField]
    private GameObject _ballPrefab;

    [SerializeField]
    private InputActionReference _touchActionRef, _touchPositionActionRef;

    private void OnEnable()
    {
        _touchActionRef.action.performed += OnTouch;
    }

    private void OnDisable()
    {
        _touchActionRef.action.performed -= OnTouch;
    }

    private void OnTouch(InputAction.CallbackContext context)
    {
        var position = _touchPositionActionRef.action.ReadValue<Vector2>();
        if (IsTouchOverUI(position)) {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(position);
        if (!Physics.Raycast(ray, out RaycastHit hitInfo)) return;

        SpawnAndThrowBallTo(hitInfo.point);

        bool IsTouchOverUI(Vector2 position)
        {
            var eventData = new PointerEventData(_eventSystem)
            {
                position = position
            };

            var results = new List<RaycastResult>();
            _graphicRaycaster.Raycast(eventData, results);

            return results.Count > 0;
        }
    }

    private void SpawnAndThrowBallTo(Vector3 targetPos)
    {
        GameObject ball = Instantiate(_ballPrefab, Camera.main.transform.position, Quaternion.identity);
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 start = Camera.main.transform.position;
        Vector3 end = targetPos;

        DebugMessageManager.Instance.AddMessage($"Camera position is: {Camera.main.transform.position}");
        DebugMessageManager.Instance.AddMessage($"Target position is: {targetPos}");

        // 計算水平距離和垂直距離
        Vector3 horizontal = new Vector3(end.x - start.x, 0, end.z - start.z);
        float distance = horizontal.magnitude;
        float height = end.y - start.y;

        float gravity = Physics.gravity.y;
        float angle = 45f * Mathf.Deg2Rad; // 你可以調整角度

        DebugMessageManager.Instance.AddMessage($"Distance: {distance}, Height: {height}");
        DebugMessageManager.Instance.AddMessage($"Gravity: {gravity}, Angle (radians): {angle}");

        // 計算初速度
        float velocity = Mathf.Sqrt(
            (distance * -gravity) / (Mathf.Sin(2 * angle))
        );

        // 計算拋射方向
        Vector3 direction = horizontal.normalized;
        Vector3 velocityVector = direction * velocity * Mathf.Cos(angle) + Vector3.up * velocity * Mathf.Sin(angle);

        rb.AddForce(velocityVector, ForceMode.VelocityChange);

        Destroy(ball, 10f);
    }
}
