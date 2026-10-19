using UnityEngine;
using UnityEngine.UI;

public class ARGazeUIController : MonoBehaviour
{
    [SerializeField] private Image _reticleImage; // 設定為 Filled Type (Radial 360)
    [SerializeField] private GameObject _uiRoot;  // 用來控制整個 UI 顯隱

    public void Initialize()
    {
        ToggleDisplay(false);
        ResetProgress();
    }

    public void ToggleDisplay(bool isEnable)
    {
        _uiRoot.SetActive(isEnable);
    }

    public void SetProgress(float progress)
    {
        // progress 0.0 ~ 1.0
        if (_reticleImage != null) {
            _reticleImage.fillAmount = progress;
        }
    }

    public void ResetProgress()
    {
        SetProgress(0f);
    }
}
