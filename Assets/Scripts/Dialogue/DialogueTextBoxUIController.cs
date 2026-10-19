using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class DialogueTextBoxUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject _displayObj;
    public bool IsDisplayActive => _displayObj.activeSelf;

    [SerializeField]
    private TMP_Text _dialogueText;

    public bool _isTextBoxWorldCanvas = false;

    private RectTransform _rectTransform;
    private Transform _parentTrans;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _parentTrans = transform.parent;
    }

    private void LateUpdate()
    {
        // 讓對話框面向攝影機
        // 但是以y軸旋轉為主，不會傾斜
        if (!_isTextBoxWorldCanvas || !IsDisplayActive) return;

        /*var camTrans = Camera.main.transform;
        Vector3 lookAtPos = new Vector3(
            camTrans.position.x,
            transform.position.y,
            camTrans.position.z
        );
        transform.LookAt(lookAtPos);*/

        // 攝影機位置轉換到父物件本地座標
        Vector3 localCamPos = _parentTrans.InverseTransformPoint(Camera.main.transform.position);
        Vector3 localDir = localCamPos - transform.localPosition;
        localDir.y = 0; // 只考慮水平方向
        if (localDir.sqrMagnitude > 0.001f) {
            // 只在本地y軸旋轉
            Quaternion targetRot = Quaternion.LookRotation(localDir, Vector3.up);
            transform.localRotation = targetRot;
        }
    }

    public void Initialize()
    {
        SetText(string.Empty);
        ToggleDisplay(false);
    }

    public void ToggleDisplay(bool isEnable)
    {
        _displayObj.SetActive(isEnable);
    }

    public void SetLocalPosition(Vector2 localPos)
    {
        _rectTransform.anchoredPosition = localPos;
    }

    public void SetPositionByScreenPoint(Vector2 screenPosition)
    { 
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform.parent as RectTransform,
            screenPosition,
            null,
            out screenPosition);
        _rectTransform.anchoredPosition = screenPosition;
    }

    public void SetText(string text)
    {
        _dialogueText.text = text;
    }

    public async Task SetTextWithTypewriterEffectAsync(string text, float delaySecPerCharacter = 0.1f, CancellationToken token = default)
    {
        _dialogueText.text = "";
        foreach (char c in text) {
            // 若被多次呼叫，會傳入ct，而在這邊終止正在運行的loop，回傳Exception讓外層catch。
            token.ThrowIfCancellationRequested();

            _dialogueText.text += c;
            await Task.Delay((int)(delaySecPerCharacter*1000), cancellationToken: token);
        }
    }
}
