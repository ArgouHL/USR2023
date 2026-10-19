using TMPro;
using UnityEngine;

public class DebugMessageManager : MonoBehaviour
{
    public static DebugMessageManager Instance { get; private set; }

    [SerializeField]
    private TMP_Text _debugMessageTextBox;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private string _lastMessage = "";
    private int _repeatCount = 1;
    public void AddMessage(string message, bool isCritical = false)
    {
        if (isCritical) {
            message = $"<color=red>{message}</color>";
        }

        if (_debugMessageTextBox.text.EndsWith(_lastMessage + (_repeatCount > 1 ? $" x{_repeatCount}" : "") + "\n")) {
            if (message == _lastMessage) {
                _repeatCount++;
                // 移除上一行
                int lastLineIndex = _debugMessageTextBox.text.LastIndexOf('\n', _debugMessageTextBox.text.Length - 2);
                _debugMessageTextBox.text = _debugMessageTextBox.text.Substring(0, lastLineIndex + 1);
                _debugMessageTextBox.text += $"{_lastMessage} x{_repeatCount}\n";
                return;
            }
            else {
                _repeatCount = 1;
            }
        }
        else {
            _repeatCount = 1;
        }

        _lastMessage = message;
        _debugMessageTextBox.text += message + "\n";
    }

    public void ClearMessages()
    {
        _debugMessageTextBox.text = "";
        _lastMessage = "";
        _repeatCount = 1;
    }
}
