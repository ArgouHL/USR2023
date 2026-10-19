using UnityEngine;
using System.Threading.Tasks;

public class SceneContext : MonoBehaviour
{
    public static SceneContext Instance { get; private set; }
    public TrackResultPrefabManager trackResultPrefabManager;
    public SimpleCharacterControllerPairManager simpleCharacterControllerPairManager;
    public ARGazeInteractionManager aRGazeInteraction;
    public CameraGazeToTargetDetector cameraGazeToTargetDetector;
    public SimpleTrackableTargetIndicatorManager simpleTrackableTargetIndicatorManager;

    public DialogueTextBoxUIController _screenDialogueTextBoxController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _screenDialogueTextBoxController.Initialize();
        aRGazeInteraction.Initialize();
    }

    public async Task PlayScreenDialgueTextWithTextAsync(string text, float waitDuration)
    {
        _screenDialogueTextBoxController.ToggleDisplay(true);
        await _screenDialogueTextBoxController.SetTextWithTypewriterEffectAsync(text);
        await Task.Delay((int)(waitDuration * 1000));
        _screenDialogueTextBoxController.ToggleDisplay(false);
    }

    public async void PlayScreenDialgueTextWithText(string text) // 不自動關閉
    {
        _screenDialogueTextBoxController.ToggleDisplay(true);
        await _screenDialogueTextBoxController.SetTextWithTypewriterEffectAsync(text);
        // 需要手動關閉！
    }
    /*
    public async Task PlayScreenDialogueTextWithScriptAsync(SimpleDialogueAnimationScript_SO script)
    {
        _screenDialogueTextBoxController.ToggleDisplay(true);

        CancellationTokenSource ctsToUse;

        if (_playScreenDialogueTextCts != null) {
            _playScreenDialogueTextCts?.Cancel();
            _playScreenDialogueTextCts?.Dispose();
        }
        _playScreenDialogueTextCts = new CancellationTokenSource();
        ctsToUse = _playScreenDialogueTextCts;

        try {
            foreach (var data in script.ScriptDataList) {

                ctsToUse.Token.ThrowIfCancellationRequested();

                // 如果使用螢幕文字盒，會讓CharacterName成為文字盒的說話者標籤
                //
                // _screenDialogueTextBoxController.SetSpeakerTag(data.CharacterName);

                await _screenDialogueTextBoxController.SetTextWithTypewriterEffectAsync(data.Line, token: ctsToUse.Token);
                await Task.Delay((int)(data.WaitDurationAfterLine * 1000), cancellationToken: ctsToUse.Token);
            }

            // 只有「正常執行完畢」才關閉 UI
            _screenDialogueTextBoxController.ToggleDisplay(false);
        } 
        catch (OperationCanceledException) {
        }
        finally {
            ctsToUse.Dispose();

            if (_playScreenDialogueTextCts == ctsToUse) {
                _playScreenDialogueTextCts = null;
            }
        }
    }
    */

    public void DisableScreenDialogueText()
    {
        _screenDialogueTextBoxController.ToggleDisplay(false);
    }
    
}
