using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using System.Threading;
using System;


//
//
// Obsoleted!
// Obsoleted!
// Obsoleted!
// Obsoleted!
//
//
public class Part1LevelManager : MonoBehaviour
{
    [SerializeField]
    private ARTrackedImageManager _arTrackedImageManager;

    [SerializeField]
    private TrackResultPrefabManager _trackResultPrefabManager;

    [SerializeField]
    private Transform _characterSpawnTrans, _characterSpawnTrans_ViewModel;
    [SerializeField]
    private GameObject _characterPrefab, _characterPrefab_ViewModel;
    private DialogueCharacterController _characterController, _characterController_ViewModel;

    [SerializeField]
    private Transform _dialogueTextBoxUISpawnTrans, _viewModelTargetPointTrans;
    [SerializeField]
    private GameObject _dialogueTextBoxUIPrefab;
    private DialogueTextBoxUIController _dialogueTextBoxUIController, _dialogueTextBoxUIController_ViewModel;

    [SerializeField]
    private GameObject _resetScanningButton;

    [SerializeField]
    private string _mainTargetImageName;

    [SerializeField]
    SimpleDialogueAnimationScript_SO _initialGuideScript, _resetScanningScript, _event1Script;

    private void Start() // 應視為是這個場景的進入點
    {
        _trackResultPrefabManager.OnSpawnedObjectByImageNameEnable += OnSpawnedObjectByImageEnable;
        _trackResultPrefabManager.OnSpawnedObjectByImageNameDisable += OnSpawnedObjectByImageDisable;

        DebugMessageManager.Instance.AddMessage("Part1LevelManager Start");

        _arTrackedImageManager.enabled = false;
        _resetScanningButton.SetActive(false);

        _characterController = Instantiate(_characterPrefab, _characterSpawnTrans).
            GetComponent<DialogueCharacterController>();
        _characterController.Initialize();
        _characterController_ViewModel = Instantiate(_characterPrefab_ViewModel, _characterSpawnTrans_ViewModel).
            GetComponent<DialogueCharacterController>();
        _characterController_ViewModel.Initialize();

        _dialogueTextBoxUIController = Instantiate(_dialogueTextBoxUIPrefab, _dialogueTextBoxUISpawnTrans).
            GetComponent<DialogueTextBoxUIController>();
        _dialogueTextBoxUIController.Initialize();
        _dialogueTextBoxUIController_ViewModel = Instantiate(_dialogueTextBoxUIPrefab, _dialogueTextBoxUISpawnTrans).
            GetComponent<DialogueTextBoxUIController>();
        _dialogueTextBoxUIController_ViewModel.SetLocalPosition(_viewModelTargetPointTrans.localPosition);
        _dialogueTextBoxUIController_ViewModel.Initialize();

        StartLevelFlow();
    }

    private void OnDestroy()
    {
        _trackResultPrefabManager.OnSpawnedObjectByImageNameEnable -= OnSpawnedObjectByImageEnable;
        _trackResultPrefabManager.OnSpawnedObjectByImageNameDisable -= OnSpawnedObjectByImageDisable;
    }

    private async void StartLevelFlow()
    {
        DebugMessageManager.Instance.AddMessage("Part1LevelManager StartLevelFlow");

        await StartOpeningGuide();
    }

    private async Task StartOpeningGuide()
    {
        await Task.Delay(1000);

        _dialogueTextBoxUIController_ViewModel.ToggleDisplay(true);

        _characterController_ViewModel.ToggleEnable(true);
        _characterController_ViewModel.PlayAnimationClipByName("Idle");

        try {
            await PlayScriptByType(true, _initialGuideScript);

            // 只有正常結束才會關閉
            _dialogueTextBoxUIController_ViewModel.ToggleDisplay(false);
        }
        catch ( OperationCanceledException ) {
        }

        _arTrackedImageManager.enabled = true;
        DebugMessageManager.Instance.AddMessage("Can scan now!");
    }

    private Coroutine _syncTextBoxPosCoroutine;
    private IEnumerator TrackCharacterPositionSyncTextBoxPosition()
    {
        while ( true ) {
            Vector3 screenPos = Camera.main.WorldToScreenPoint
                (_characterController.DialogueTextBoxUITargetPointTrans.position);
            // transfer screen point to UI local point

            _dialogueTextBoxUIController.SetPositionByScreenPoint(screenPos);
            yield return null;
        }
    }

    public async void OnSpawnedObjectByImageEnable(string imageName, GameObject obj)
    {
        if ( imageName != _mainTargetImageName) return;

        _resetScanningButton.SetActive(true);

        _characterController.transform.parent = obj.transform;
        _characterController.transform.localPosition = Vector3.zero;
        _characterController.transform.rotation = Quaternion.identity;
        _characterController.ToggleEnable(true);
        
        _characterController_ViewModel.ToggleEnable(false);

        _dialogueTextBoxUIController_ViewModel.ToggleDisplay(false);
        _dialogueTextBoxUIController.ToggleDisplay(true);
        _syncTextBoxPosCoroutine = StartCoroutine(TrackCharacterPositionSyncTextBoxPosition());

        try {
            await PlayScriptByType(false, _event1Script);

            _dialogueTextBoxUIController.ToggleDisplay(false);
            // 只有正常結束才會關閉
        } catch ( OperationCanceledException ) {
        }
    }

    public async void OnSpawnedObjectByImageDisable(string imageName, GameObject obj)
    {
        if ( imageName != _mainTargetImageName) return;

        _resetScanningButton.SetActive(false);

        _characterController_ViewModel.ToggleEnable(true);

        _dialogueTextBoxUIController_ViewModel.ToggleDisplay(true);
        _dialogueTextBoxUIController.ToggleDisplay(false);
        if ( _syncTextBoxPosCoroutine != null) {
            StopCoroutine(_syncTextBoxPosCoroutine);
        }

        try {
            await PlayScriptByType(true, _resetScanningScript);

            _dialogueTextBoxUIController_ViewModel.ToggleDisplay(false);
        }
        catch ( OperationCanceledException) {
        }
    }

    public void StartFirstEvent()
    {
        // 開始後就不能再重設掃描
        _resetScanningButton.SetActive(false); 


    }

    CancellationTokenSource _playScriptCts, _playScriptCts_ViewMode;
    private async Task PlayScriptByType(bool isViewModel, SimpleDialogueAnimationScript_SO script)
    {
        DialogueCharacterController characterCon;
        DialogueTextBoxUIController textBoxCon;

        CancellationTokenSource ctsToUse;

        if (isViewModel) {
            if (_playScriptCts_ViewMode != null) {
                _playScriptCts_ViewMode?.Cancel();
                _playScriptCts_ViewMode?.Dispose();
            }
            _playScriptCts_ViewMode = new CancellationTokenSource();
            ctsToUse = _playScriptCts_ViewMode;

            characterCon = _characterController_ViewModel;
            textBoxCon = _dialogueTextBoxUIController_ViewModel;
        }
        else {
            if (_playScriptCts != null) {
                _playScriptCts?.Cancel();
                _playScriptCts?.Dispose();
            }
            _playScriptCts = new CancellationTokenSource();
            ctsToUse = _playScriptCts;

            characterCon = _characterController;
            textBoxCon = _dialogueTextBoxUIController;
        }

        try {
            foreach (var data in script.ScriptDataList) {

                ctsToUse.Token.ThrowIfCancellationRequested();

                await textBoxCon.SetTextWithTypewriterEffectAsync(data.Line, token: ctsToUse.Token);
                await Task.Delay((int)(data.WaitDurationAfterLine * 1000), cancellationToken: ctsToUse.Token);
            }
        } // 在內部不捕捉 OperationCanceledException，讓外層知道任務被取消
        finally {
            ctsToUse.Dispose();

            //    (增加一個判斷是為了防止競態條件：
            //     如果在此期間又有新任務啟動，我們不應將新任務的 CTS 設為 null)
            if (isViewModel && _playScriptCts_ViewMode == ctsToUse) {
                _playScriptCts_ViewMode = null;
            }
            else if (!isViewModel && _playScriptCts == ctsToUse) {
                _playScriptCts = null;
            }
        }
    }
}
