using System.Threading;
using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;

public class Part2GameManager : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager _arTrackedImageManager;
    [SerializeField] private TrackResultPrefabManager _trackResultPrefabManager;
    [SerializeField] private SimpleCharacterControllerPairManager _characterAndTextBoxControllerPairManager;
    [SerializeField] private GameObject _resetScanningButton;
    [SerializeField] private string _mainTargetImageName;
    [SerializeField] SimpleDialogueAnimationScript_SO _initialGuideScript, _resetScanningScript;
    Coroutine _GameCoroutine;

    [SerializeField] Part2MiniGameController miniGameController;

    IEnumerator GameCoroutine()
    {
        // 顯示西門寶寶、傳送門、介紹
        // 進入傳送門
        // 人群對話
        // 人群消失
        Task task = _characterAndTextBoxControllerPairManager.PlayScriptAsync(_resetScanningScript);
        yield return new WaitUntil(() => task.IsCompleted);
        miniGameController = FindObjectOfType<Part2MiniGameController>();
        miniGameController.StartGame();
        yield return new WaitUntil(() => miniGameController.isGameOver());

    }

    private void Start() // 應視為是這個場景的進入點
    {
        _trackResultPrefabManager.OnSpawnedObjectByImageNameEnable += OnSpawnedObjectByImageEnable;
        _trackResultPrefabManager.OnSpawnedObjectByImageNameDisable += OnSpawnedObjectByImageDisable;

        DebugMessageManager.Instance.AddMessage("Part1GameManager Start");

        _arTrackedImageManager.enabled = false;
        _resetScanningButton.SetActive(false);

        _characterAndTextBoxControllerPairManager.Initialize();

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

        //
        // do opening guide 
        //

        _arTrackedImageManager.enabled = true;
        DebugMessageManager.Instance.AddMessage("Can scan now!");
    }

    public async void OnSpawnedObjectByImageEnable(string imageName, GameObject spawnedObj)
    {
        if (imageName != _mainTargetImageName) return;

        _resetScanningButton.SetActive(true);

        var characterObjContainer = spawnedObj.GetComponent<SimpleCharacterContainer>();
        _characterAndTextBoxControllerPairManager.SetupCharacterControllerPair
            (characterObjContainer.CharacterObjNamePairs);

        try {
            await _characterAndTextBoxControllerPairManager.
                PlayScriptAsync(_initialGuideScript);
            _GameCoroutine = StartCoroutine(GameCoroutine());
        } catch (OperationCanceledException) {
        }
    }

    public async void OnSpawnedObjectByImageDisable(string imageName, GameObject spawnedObj)
    {
        if (imageName != _mainTargetImageName) return;

        _resetScanningButton.SetActive(false);

        try {
            await _characterAndTextBoxControllerPairManager.
                PlayScriptAsync(_resetScanningScript);
            StopCoroutine(_GameCoroutine);
        } catch (OperationCanceledException) {
        }
    }

    public void StartFirstEvent()
    {
        // 開始後就不能再重設掃描
        _resetScanningButton.SetActive(false);
    }
}

