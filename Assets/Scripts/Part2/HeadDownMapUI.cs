using UnityEngine;
using UnityEngine.Events;

public class HeadDownMapUI : MonoBehaviour
{
    [Header("Refs")]
    public Transform cameraTransform;          // 玩家相機 (FPS Camera)
    public RectTransform mapPanel;             // 地圖面板(要移動的)
    [Tooltip("要旋轉的目標(可指定 mapPanel 或 mapPanel 內的 Image)")]
    public RectTransform mapRotateTarget;      // 要旋轉的 UI

    [Header("Positions (anchoredPosition)")]
    public Vector2 hiddenPos = new Vector2(0, -600);
    public Vector2 shownPos  = new Vector2(0, 80);

    [Header("Angle - Show/Hide")]
    public float showPitchDeg = 55f;
    public float hidePitchDeg = 45f;

    [Header("Rotation by pitch (Z rotation)")]
    [Tooltip("開始帶動旋轉的低頭角度")]
    public float rotateStartPitchDeg = 30f;

    [Tooltip("低頭到這個角度時，旋轉達到最大")]
    public float rotateEndPitchDeg = 80f;

    [Tooltip("低頭較少時的Z旋轉角度(度)")]
    public float minRotDeg = 0f;

    [Tooltip("低頭很多時的Y旋轉角度(度)")]
    public float maxRotDeg = 25f;

    [Header("Animation")]
    public float smoothSpeed = 10f;

    [Header("On map viewed then look up")]
    [Tooltip("完成一次：低頭看地圖(顯示) → 抬頭收回(隱藏) 後觸發")]
    public UnityEvent onViewedAndLookUp;

    public bool looked = false;

    private bool isShown;
    private bool hasTriggered;
    private Vector2 posVelocity;       // SmoothDamp 用
    private float rotVelocity;         // SmoothDampAngle 用

    void Reset()
    {
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
    }

    void Awake()
    {
        if (!cameraTransform) cameraTransform = Camera.main.transform;
    }

    void Start()
    {
        if (!mapRotateTarget) mapRotateTarget = mapPanel;

        mapPanel.anchoredPosition = hiddenPos;
        isShown = false;
    }

    void Update()
    {
        float pitchDown = GetPitchDownDegrees(cameraTransform.eulerAngles.x);

        bool wasShown = isShown; // 記住上一幀狀態

        // 滯後：進入顯示用 showPitchDeg，退出用 hidePitchDeg
        if (!isShown && pitchDown >= showPitchDeg) isShown = true;
        else if (isShown && pitchDown <= hidePitchDeg) isShown = false;

        // ✅ 這裡抓「成功看一次並抬頭」：從 shown 變成 hidden 的瞬間
        if (wasShown && !isShown)
        {
            if (!hasTriggered)
            {
                hasTriggered = true;
                onViewedAndLookUp?.Invoke();
                looked = true;
                // 你也可以改成呼叫一個方法，例如：OnViewedAndLookUp();
            }
        }

        // 位置平滑
        Vector2 targetPos = isShown ? shownPos : hiddenPos;
        mapPanel.anchoredPosition = Vector2.SmoothDamp(
            mapPanel.anchoredPosition,
            targetPos,
            ref posVelocity,
            1f / smoothSpeed
        );

        // 旋轉：把低頭角度映射到 X 旋轉
        float t = Mathf.InverseLerp(rotateStartPitchDeg, rotateEndPitchDeg, pitchDown);
        float targetX = Mathf.Lerp(minRotDeg, maxRotDeg, t);

        float currentX = mapRotateTarget.localEulerAngles.x;
        float smoothX = Mathf.SmoothDampAngle(
            currentX,
            targetX,
            ref rotVelocity,
            1f / smoothSpeed
        );

        mapRotateTarget.localRotation = Quaternion.Euler(smoothX, 0f, 0f);
    }

    // Unity 的 eulerAngles.x 會在 0~360 轉，這裡轉成「往下看」的角度
    private float GetPitchDownDegrees(float eulerX)
    {
        float x = eulerX;
        if (x > 180f) x -= 360f;   // 變成 -180~180
        return Mathf.Max(0f, x);   // x > 0 往下看
    }
}