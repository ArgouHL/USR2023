using System.Threading;
using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private ARTrackedImageManager _arTrackedImageManager;
    [SerializeField]
    private TrackResultPrefabManager _trackResultPrefabManager;
    [SerializeField]
    private SimpleCharacterControllerPairManager _characterAndTextBoxControllerPairManager;

    [SerializeField]
    private GameObject _cameraObjectInPortalDisplayMaskObj;

    [SerializeField]
    private GameObject _resetScanningButton;

    [SerializeField]
    private DialogueTextBoxUIController _screenDialogueTextBoxController;

    [SerializeField]
    private string _mainTargetImageName;


    private SimplePortalRelatedController _currentPortalRelatedController;

    private void Start()
    {
        _trackResultPrefabManager.OnSpawnedObjectByImageNameEnable += OnSpawnedObjectByImageEnable;
        _trackResultPrefabManager.OnSpawnedObjectByImageNameDisable += OnSpawnedObjectByImageDisable;

        DebugMessageManager.Instance.AddMessage("GameManager Start");

        _arTrackedImageManager.enabled = false;
        _resetScanningButton.SetActive(false);

        _characterAndTextBoxControllerPairManager.Initialize();
        _screenDialogueTextBoxController.Initialize();

        _cameraObjectInPortalDisplayMaskObj.SetActive(false);

        StartOpeningGuide();
    }

    private void OnDestroy()
    {
        _trackResultPrefabManager.OnSpawnedObjectByImageNameEnable -= OnSpawnedObjectByImageEnable;
        _trackResultPrefabManager.OnSpawnedObjectByImageNameDisable -= OnSpawnedObjectByImageDisable;
    }

    private /*async*/ void StartOpeningGuide()
    {
        DebugMessageManager.Instance.AddMessage("Part1LevelManager StartLevelFlow");

        // await PlayScreenDialogueTextWithScriptAsync(_initialGuideScript);

        _arTrackedImageManager.enabled = true;
        DebugMessageManager.Instance.AddMessage("Can scan now!");
    }

    public /*async*/ void OnSpawnedObjectByImageEnable(string imageName, GameObject spawnedObj)
    {
        if (imageName != _mainTargetImageName) return;

        DebugMessageManager.Instance.AddMessage(imageName + " spawned!");
        _resetScanningButton.SetActive(true);

        var characterObjContainer = spawnedObj.GetComponent<SimpleCharacterContainer>();
        _characterAndTextBoxControllerPairManager.SetupCharacterControllerPair
            (characterObjContainer.CharacterObjNamePairs);
        //_currentPortalRelatedController = spawnedObj.GetComponent<SimplePortalRelatedController>();
        //_currentPortalRelatedController.InitializeAndSetReference(this);

        DebugMessageManager.Instance.AddMessage("Play guild to start script");

        // await PlayScreenDialogueTextWithScriptAsync(_guildToStartScript);
    }

    public async void OnSpawnedObjectByImageDisable(string imageName, GameObject spawnedObj)
    {
        if (imageName != _mainTargetImageName) return;

        _resetScanningButton.SetActive(false);

    }



    CancellationTokenSource _playScreenDialogueTextCts;
    private async Task PlayScreenDialogueTextWithScript(SimpleDialogueAnimationScript_SO script)
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

    private async Task PlayScreenDialgueTextWithText(string text, float waitDuration)
    {
        _screenDialogueTextBoxController.ToggleDisplay(true);
        await _screenDialogueTextBoxController.SetTextWithTypewriterEffectAsync(text);
        await Task.Delay((int)(waitDuration * 1000));
        _screenDialogueTextBoxController.ToggleDisplay(false);
    }


}
