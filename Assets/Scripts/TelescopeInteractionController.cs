using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TelescopeInteractionController : MonoBehaviour
{
    [SerializeField]
    private SimplePinchDetector _pinchDetector;

    [SerializeField]
    private Camera _mainCamera,
        _telescopeCamera;

    [SerializeField]
    private GameObject _telescopeModelGraphxObj;
    [SerializeField]
    private Animator _telescopeModelAnimator;

    [SerializeField]
    private GameObject _telescopeViewObj;
    [SerializeField]
    private CanvasGroup _telescopeTransitionBlockerCG;

    [SerializeField]
    private GameObject _telescopeTransitionUIRoot,
        _telescopeInteractionUIRoot;
    [SerializeField]
    private Slider _zoomSlider;

    public void Initialize()
    {
        _pinchDetector.ToggleDetecting(false);

        _mainCamera.enabled = true;
        _telescopeCamera.enabled = false;

        _telescopeViewObj.SetActive(false);
        _telescopeTransitionBlockerCG.alpha = 0f;

        ToggleInteractable(false);

        _pinchDetector.OnZoomDeltaChanged += OnPinchValueDeltaChange;
    }

    private void OnDestroy()
    {
        _pinchDetector.OnZoomDeltaChanged -= OnPinchValueDeltaChange;
    }

    public async Task ToggleTelescopeAsync(bool isEnable)
    {
        if (isEnable) _telescopeModelAnimator.Play("Telescope_Enable");

        await Task.Delay(500); // 0.5sec

        // fade in transition blocker in 1sec
        _telescopeTransitionBlockerCG.alpha = 0f;
        float duration = 1f;
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            _telescopeTransitionBlockerCG.alpha = Mathf.Clamp01(elapsed / duration);
            await Task.Yield();
        }
        _telescopeTransitionBlockerCG.alpha = 1f;

        _mainCamera.enabled = !isEnable;
        _telescopeCamera.enabled = isEnable;

        _telescopeViewObj.SetActive(isEnable);
        ToggleInteractable(isEnable);

        if (!isEnable) {
            DebugMessageManager.Instance.AddMessage("已離開望遠鏡視角");
            _telescopeModelAnimator.Play("Telescope_Disable");
        }

        // fade out transition blocker in 0.5sec
        duration = 1f;
        elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            _telescopeTransitionBlockerCG.alpha = 1f - Mathf.Clamp01(elapsed / duration);
            await Task.Yield();
        }
        _telescopeTransitionBlockerCG.alpha = 0f;
    }

    private void ToggleInteractable(bool isInteractable)
    {
        _telescopeInteractionUIRoot.SetActive(isInteractable);

        _pinchDetector.ToggleDetecting(isInteractable);

        _zoomSlider.value = 0f;
    }

    public void OnSliderChange(float value) // called by Slider
    {
        // value 0~1

        // FOV 預設 60
        // 要放大 最大到 10 (數值越小 代表視野越窄 看起來越大)

        float targetFOV = Mathf.Lerp(60f, 5f, value);
        _telescopeCamera.fieldOfView = targetFOV;
    }

    public void OnPinchValueDeltaChange(float delta) // called by SimplePinchDetector
    {
        // 會每一幀呼叫，delta 可能是正負值

        float sliderValue = _zoomSlider.value;
        sliderValue += delta; // delta 已經乘過靈敏度了
        sliderValue = Mathf.Clamp01(sliderValue);
        _zoomSlider.value = sliderValue;
    }


}
