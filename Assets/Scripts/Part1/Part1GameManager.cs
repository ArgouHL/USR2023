using System.Threading;
using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class Part1GameManager : MonoBehaviour, IPlayerEnterPortalEventPassable
{
    [SerializeField]
    private ARTrackedImageManager _arTrackedImageManager;
    [SerializeField]
    private TrackResultPrefabManager _trackResultPrefabManager;
    [SerializeField]
    private SimpleCharacterControllerPairManager _characterAndTextBoxControllerPairManager;
    [SerializeField]
    private ARGazeInteractionManager _arGazeInteractionManager; // 用於處理玩家攝影機視線互動
    [SerializeField]
    private SimpleTrackableTargetIndicatorManager _trackableTargetIndicatorManager; // 用於處理目標方向指示器
    public SimpleTrackableTargetIndicatorManager TrackableTargetIndicatorManager => _trackableTargetIndicatorManager;
    [SerializeField]
    private TelescopeInteractionController _telescopeInteractionController;

    /*[SerializeField]
    private GameObject _cameraObjectInPortalDisplayMaskObj;*/

    [SerializeField]
    private GameObject _resetScanningButton,
        _dropTelescopeButton;

    [SerializeField]
    private DialogueTextBoxUIController _screenDialogueTextBoxController;

    [SerializeField]
    private List<string> _targetImageNameList;

    [Header("Dialogue Scripts")]
    [SerializeField]
    SimpleDialogueAnimationScript_SO _scenario_1_1;
    [SerializeField]
    SimpleDialogueAnimationScript_SO
        _scenario_1_2,
        _scenario_2_0,
        _scenario_2_1,
        _scenario_Telescope_Castle,
        _scenario_Telescope_Gate,
        _scenario_3_1,
        _scenario_CastleWorker,
        _scenario_Gabezo_A,
        _scenario_Gabezo_B;

    private SimplePortalRelatedController _currentPortalRelatedController;
    private Part1SpawnPrefabBridge _currentSpawnPrefabBridge;

    private bool _isMainFlowEnded = false; // 標記主要流程結束，掃描望遠鏡不同功能

    private UniversalAdditionalCameraData _cameraData;

    // 視為是這個場景的進入點，先進行初始化
    private void Start() 
    {
        _cameraData = Camera.main.GetUniversalAdditionalCameraData();
        _cameraData.SetRenderer(0);

        _trackResultPrefabManager.OnSpawnedObjectByImageNameEnable += OnSpawnedObjectByImageEnable;
        _trackResultPrefabManager.OnSpawnedObjectByImageNameDisable += OnSpawnedObjectByImageDisable;

        DebugMessageManager.Instance.AddMessage("Part1GameManager Start");

        _arTrackedImageManager.enabled = false;
        _resetScanningButton.SetActive(false);

        _characterAndTextBoxControllerPairManager.Initialize();
        _screenDialogueTextBoxController.Initialize();

        _arGazeInteractionManager.Initialize();
        _arGazeInteractionManager.OnTargetTriggered += HandleGazeTrigger;

        _trackableTargetIndicatorManager.Initialize();
        _trackableTargetIndicatorManager.ToggleActive(true);
        // 以目前來說 trackableTargetIndicatorManager 預設保持啟動也無所謂，因為個別Target會有自己的ToggleAPI來顯示/關閉顯示
        // 但如果如 GazeInteractionManager 一樣有啟動/關閉互動的UI上需求，則仍可以透過 ToggleActive 來控制Manager本身的啟動狀態。

        _telescopeInteractionController.Initialize();

        // _cameraObjectInPortalDisplayMaskObj.SetActive(false);

        StartOpeningGuide();
    }

    private void OnDestroy()
    {
        _trackResultPrefabManager.OnSpawnedObjectByImageNameEnable -= OnSpawnedObjectByImageEnable;
        _trackResultPrefabManager.OnSpawnedObjectByImageNameDisable -= OnSpawnedObjectByImageDisable;

        _arGazeInteractionManager.OnTargetTriggered -= HandleGazeTrigger;
    }

    // 初始化完成，正式進入引導與玩家操作流程
    private /*async*/ void StartOpeningGuide() 
    {
        DebugMessageManager.Instance.AddMessage("Part1LevelManager StartLevelFlow");

        // await PlayScreenDialogueTextWithScriptAsync(_initialGuideScript);

        _arTrackedImageManager.enabled = true;
        DebugMessageManager.Instance.AddMessage("Can scan now!");
    }
    
    // 當主要目標圖像被掃描到，並生成對應物件時
    private async void OnSpawnedObjectByImageEnable(string imageName, GameObject spawnedObj)
    {
        if (!_targetImageNameList.Contains(imageName)) return;

        DebugMessageManager.Instance.AddMessage(imageName + " spawned!");
        _resetScanningButton.SetActive(true);

        //
        // 生成場景物件之後，初始化其上的腳本 
        // 
        var characterObjContainer = spawnedObj.GetComponent<SimpleCharacterContainer>();
        _currentPortalRelatedController = spawnedObj.GetComponent<SimplePortalRelatedController>();
        _currentSpawnPrefabBridge = spawnedObj.GetComponent<Part1SpawnPrefabBridge>();

        _characterAndTextBoxControllerPairManager.SetupCharacterControllerPair
            (characterObjContainer.CharacterObjNamePairs);
        _currentPortalRelatedController.InitializeAndSetReference(this);
        _currentSpawnPrefabBridge.InitializeAndSetReference(this);

        _currentSpawnPrefabBridge.FootStepController_1.gameObject.SetActive(true);
        _currentSpawnPrefabBridge.FootStepController_1.Show();

        await Task.Delay((int)(0.5f * 1000));

        PlayScreenDialgueTextWithText("請將攝影機看向我~");
        _arGazeInteractionManager.ToggleGazeInteraction(true);
        _currentSpawnPrefabBridge.GazeTarget_LW.ToggleInteractable(true);

        // 顯示小西門角色
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("LW", true);
        await _characterAndTextBoxControllerPairManager.ToggleCharacterDisplayWithDissolveEffectAsync("LW", true);

        // 等待玩家將目光對準小西門角色。（由 ARGazeInteractionManager 處理）
        // => 觸發 targetID 為 Guide 的事件
        // => 呼叫 HandleGazeTrigger()
        // => 呼叫 StartScenario1()，接續流程

        // DebugMessageManager.Instance.AddMessage("Play guild to start script");
        // await PlayScreenDialogueTextWithScriptAsync(_guildToStartScript);
    }

    // 由trackResultPrefabManager呼叫，目前僅會由ResetScanningButton按鈕呼叫觸發，因為arTrackedImageManager的【失去追蹤】機制尚不明確。
    private /*async*/ void OnSpawnedObjectByImageDisable(string imageName, GameObject spawnedObj)
    {
        if (!_targetImageNameList.Contains(imageName)) return;

        _resetScanningButton.SetActive(false);

        DisableScreenDialogueText();

        // await PlayScreenDialogueTextWithScriptAsync(_resetScanningScript);
    }

    // 從【開始】到【進入傳送門】的流程
    private async void StartScenario1() 
    {
        DebugMessageManager.Instance.AddMessage("Start Scenario 1");

        // 開始後就不能再重設掃描
        _resetScanningButton.SetActive(false);

        DisableScreenDialogueText();
        _arGazeInteractionManager.ToggleGazeInteraction(false);

        _currentSpawnPrefabBridge.FootStepController_1.Hide();

        // 
        // 從這邊開始就是所謂【Scenario】編寫的基本樣式
        // Script講話、撥放動畫 等待結束後
        // 執行 指令、await 等動作後
        // 再由下一個 Script接續
        // 直到下一個 Scenario 之前 （需要玩家介入操作時） 才會中斷
        //
        // 也就是說，一個Scenario，會在玩家無法操作的情況下連續完成
        // 

        await _characterAndTextBoxControllerPairManager.
            PlayScriptAsync(_scenario_1_1);

        await Task.Delay(1000);

        // 啟動傳送門動畫
        var portalAnimator = _currentSpawnPrefabBridge.PortalFrameAnimator;

        _currentSpawnPrefabBridge.PortalObject.SetActive(true);

        portalAnimator.Play("Portal_FallFromSky");
        portalAnimator.Update(0);
        float waitTime = portalAnimator.GetCurrentAnimatorStateInfo(0).length;
        await Task.Delay((int)(waitTime * 1000));

        portalAnimator.Play("Portal_OpenDoor");
        portalAnimator.Update(0);
        waitTime = portalAnimator.GetCurrentAnimatorStateInfo(0).length;
        await Task.Delay((int)(waitTime * 1000));

        await _currentPortalRelatedController.TogglePortalAsync(true);

        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("ShopKeeper", true);

        await _characterAndTextBoxControllerPairManager.
            PlayScriptAsync(_scenario_1_2);

        PlayScreenDialgueTextWithText("請進入時空門");
        // 等待玩家進入傳送門
        _currentSpawnPrefabBridge.FootStepController_2.gameObject.SetActive(true);
        _currentSpawnPrefabBridge.FootStepController_2.Show();

        StartCoroutine(WaitForPlayerEnterPortal());
    }

    private bool _isPlayerInPortalWorld = false;
    private IEnumerator WaitForPlayerEnterPortal()
    {
        DebugMessageManager.Instance.AddMessage("Waiting for player to enter portal world...");
    
        while (!_isPlayerInPortalWorld) {
            yield return null;
        }

        _currentSpawnPrefabBridge.FootStepController_2.Hide();

        DebugMessageManager.Instance.AddMessage("Player has entered portal world.");

        StartScenario2_0();
    }

    private async void StartScenario2_0() // 從【傳送門進入】到【看向店小二】的流程
    {
        DebugMessageManager.Instance.AddMessage("Start Scenario 2_0");

        // 將之前啟動顯示的【請進入傳送門】對話框關閉
        DisableScreenDialogueText();

        // Scenario 2_0 內容

        // 關閉時空門外的小西門角色顯示，但啟動傳送門內的小西門角色顯示
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplayWithDissolveEffect("LW", false);
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("LW", false);

        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("LW_Portal", true);
        await _characterAndTextBoxControllerPairManager.ToggleCharacterDisplayWithDissolveEffectAsync("LW_Portal", true);

        await _characterAndTextBoxControllerPairManager.
            PlayScriptAsync(_scenario_2_0);

        _arGazeInteractionManager.ToggleGazeInteraction(true);
        _currentSpawnPrefabBridge.GazeTarget_ShopKeeper.ToggleInteractable(true);
        PlayScreenDialgueTextWithText("請看向店小二");

        // 等待玩家看向店小二
        // => 觸發 targetID 為 ShopKeeper 的事件
        // => 呼叫 HandleGazeTrigger()
        // => 呼叫 StartScenario2()，接續流程
    }

    private async void StartScenario2() // 從【看向店小二】到【選取望遠鏡】的流程
    {
        DebugMessageManager.Instance.AddMessage("Start Scenario 2");

        _arGazeInteractionManager.ToggleGazeInteraction(false);

        // 將之前啟動顯示的【請看向店小二】對話框關閉
        DisableScreenDialogueText();

        // Scenario 2 內容

        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("TempleMaster", true);
        await _characterAndTextBoxControllerPairManager.
            PlayScriptAsync(_scenario_2_1);

        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("TempleMaster", false);

        _currentSpawnPrefabBridge.ToggleTelescopeSelectionUI(true);
        PlayScreenDialgueTextWithText("請瞄準望遠鏡圖示以觀察遠方"); // 不自動關閉

        _arGazeInteractionManager.ToggleGazeInteraction(true);
        _currentSpawnPrefabBridge.GazeTarget_Telescope.ToggleInteractable(true);
        // 等待玩家選取望遠鏡
        // => 觸發 targetID 為 Telescope 的事件
        // => 呼叫 HandleGazeTrigger()
        // => 呼叫 OnEnterTelescopeView_MainFlow()，接續流程
    }

    public async void OnEnterTelescopeView_MainFlow() // 處理玩家在望遠鏡視角中的操作
    {
        _arGazeInteractionManager.ToggleGazeInteraction(false);
        await _telescopeInteractionController.ToggleTelescopeAsync(true);
        _trackableTargetIndicatorManager.SwitchTrackingCameraWithID("Telescope");

        _currentSpawnPrefabBridge.GazeTarget_Telescope.ToggleInteractable(false);
        _currentSpawnPrefabBridge.ToggleTelescopeSelectionUI(false);

        DisableScreenDialogueText();

        // 啟動熱蘭遮城的視線互動 
        _arGazeInteractionManager.ToggleGazeInteraction(true);
        _currentSpawnPrefabBridge.GazeTarget_Castle.ToggleInteractable(true);
        // & 目標追蹤
        
        _currentSpawnPrefabBridge.TrackableTarget_Castle.ToggleTargetTrackable(true);

        PlayScreenDialgueTextWithText("請找出熱蘭遮城"); // 不自動關閉

        DebugMessageManager.Instance.AddMessage("Waiting for player to gaze at Castle...");
    
        // 等待玩家注視熱蘭遮城
        // => 觸發 targetID 為 Castle 的事件
        // => 呼叫 HandleGazeTrigger()
        // => 呼叫 OnSelectInTelescopeView_Castle()，接續流程
    }

    private async void OnSelectInTelescopeView_Castle()
    {
        // 關閉熱蘭遮城的視線互動
        _arGazeInteractionManager.ToggleGazeInteraction(false);
        _currentSpawnPrefabBridge.GazeTarget_Castle.ToggleInteractable(false);
        
        await PlayScreenDialogueTextWithScriptAsync(_scenario_Telescope_Castle);

        _currentSpawnPrefabBridge.TrackableTarget_Castle.ToggleTargetTrackable(false);

        // 啟動鹿耳門的視線互動
        _arGazeInteractionManager.ToggleGazeInteraction(true);
        _currentSpawnPrefabBridge.GazeTarget_Gate.ToggleInteractable(true);
        // & 目標追蹤
        _currentSpawnPrefabBridge.TrackableTarget_Gate.ToggleTargetTrackable(true);

        PlayScreenDialgueTextWithText("請找出鹿耳門港"); // 不自動關閉

        // 等待玩家將視線移到鹿耳門
        // => 觸發 targetID 為 Gate 的事件
        // => 呼叫 HandleGazeTrigger()
        // => 呼叫 OnSelectInTelescopeView_Gate()，接續流程
    }

    private async void OnSelectInTelescopeView_Gate()
    {
        // 關閉鹿耳門的視線互動
        _arGazeInteractionManager.ToggleGazeInteraction(false);
        _currentSpawnPrefabBridge.GazeTarget_Gate.ToggleInteractable(false);
        
        await PlayScreenDialogueTextWithScriptAsync(_scenario_Telescope_Gate);

        _currentSpawnPrefabBridge.TrackableTarget_Gate.ToggleTargetTrackable(false);

        _currentSpawnPrefabBridge.StartWaterLevelRiseAnimation();
        // 水面上升的動畫效果
        await PlayScreenDialgueTextWithTextAsync("請觀察水面上升的樣子", 3f);

        await PlayScreenDialgueTextWithTextAsync("你看，剛好漲潮了，那些小船終於能進港道了。", 1f);
        await PlayScreenDialgueTextWithTextAsync("快把望遠鏡收起來，看我們前面！他要的貨應該就要來了！", 1f);

        // 退出望遠鏡視角
        await _telescopeInteractionController.ToggleTelescopeAsync(false);
        _trackableTargetIndicatorManager.SwitchTrackingCameraWithID("Main");

        StartScenario3();
    }

    private async void StartScenario3()
    {
        DebugMessageManager.Instance.AddMessage("Start Scenario 3");

        // 船隻進港的動畫效果
        _currentSpawnPrefabBridge.StartPlayShipAnimation();
        // 需等待船隻進入港道
        await Task.Delay(14 * 1000);

        await _characterAndTextBoxControllerPairManager.
            PlayScriptAsync(_scenario_3_1);
        
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("ShopKeeper", false);

        await _characterAndTextBoxControllerPairManager.ToggleCharacterDisplayWithDissolveEffectAsync("LW_Portal", false);
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("LW_Portal", false);

        DebugMessageManager.Instance.AddMessage("Main flow ended.");

        EndGameFlow();
    }

    private void EndGameFlow()
    {
        _isMainFlowEnded = true;
        // 玩家可以繼續操作望遠鏡觀察

        _arGazeInteractionManager.ToggleGazeInteraction(true);

        _currentSpawnPrefabBridge.GazeTarget_Telescope.ToggleInteractable(true);
        _currentSpawnPrefabBridge.ToggleTelescopeSelectionUI(true);

        _currentSpawnPrefabBridge.GazeTarget_Gazebo_A.ToggleInteractable(true);
        _currentSpawnPrefabBridge.TrackableTarget_Gazebo_A.ToggleTargetTrackable(true);
        _currentSpawnPrefabBridge.GazeTarget_Gazebo_B.ToggleInteractable(true);
        _currentSpawnPrefabBridge.TrackableTarget_Gazebo_B.ToggleTargetTrackable(true);

        PlayScreenDialgueTextWithText("可以繼續觀察周圍(例如旁邊的涼亭)或是再使用一次望遠鏡");

        // 等待玩家選取望遠鏡
        // => 觸發 targetID 為 Telescope 的事件
        // => 呼叫 HandleGazeTrigger()
        // => 呼叫 OnEnterTelescopeView_EndGame()，接續流程
    }



    private async void OnSelect_Gabezo_A_EndGame()
    {
        // 在撥放 峴石亭 對話時，暫時關閉其他互動與追蹤
        // 但結束後重啟

        _currentSpawnPrefabBridge.GazeTarget_Telescope.ToggleInteractable(false);

        _currentSpawnPrefabBridge.GazeTarget_Gazebo_A.ToggleInteractable(false);
        _currentSpawnPrefabBridge.GazeTarget_Gazebo_B.ToggleInteractable(false);
        _currentSpawnPrefabBridge.TrackableTarget_Gazebo_B.ToggleTargetTrackable(false);

        await PlayScreenDialogueTextWithScriptAsync(_scenario_Gabezo_A);

        _currentSpawnPrefabBridge.GazeTarget_Telescope.ToggleInteractable(true);

        _currentSpawnPrefabBridge.GazeTarget_Gazebo_A.ToggleInteractable(true);
        _currentSpawnPrefabBridge.GazeTarget_Gazebo_B.ToggleInteractable(true);
        _currentSpawnPrefabBridge.TrackableTarget_Gazebo_B.ToggleTargetTrackable(true);

        PlayScreenDialgueTextWithText("可以自由觀察");
    }

    private async void OnSelect_Gabezo_B_EndGame()
    {
        // 在撥放 棠蔭亭 對話時，暫時關閉其他互動與追蹤
        // 但結束後重啟

        _currentSpawnPrefabBridge.GazeTarget_Telescope.ToggleInteractable(false);

        _currentSpawnPrefabBridge.GazeTarget_Gazebo_B.ToggleInteractable(false);
        _currentSpawnPrefabBridge.GazeTarget_Gazebo_A.ToggleInteractable(false);
        _currentSpawnPrefabBridge.TrackableTarget_Gazebo_A.ToggleTargetTrackable(false);

        await PlayScreenDialogueTextWithScriptAsync(_scenario_Gabezo_B);

        _currentSpawnPrefabBridge.GazeTarget_Telescope.ToggleInteractable(true);

        _currentSpawnPrefabBridge.GazeTarget_Gazebo_B.ToggleInteractable(true);
        _currentSpawnPrefabBridge.GazeTarget_Gazebo_A.ToggleInteractable(true);
        _currentSpawnPrefabBridge.TrackableTarget_Gazebo_A.ToggleTargetTrackable(true);

        PlayScreenDialgueTextWithText("可以自由觀察");
    }



    private async void OnEnterTelescopeView_EndGame()
    {
        _currentSpawnPrefabBridge.GazeTarget_Gazebo_A.ToggleInteractable(false);
        _currentSpawnPrefabBridge.TrackableTarget_Gazebo_A.ToggleTargetTrackable(false);
        _currentSpawnPrefabBridge.GazeTarget_Gazebo_B.ToggleInteractable(false);
        _currentSpawnPrefabBridge.TrackableTarget_Gazebo_B.ToggleTargetTrackable(false);

        _arGazeInteractionManager.ToggleGazeInteraction(false);
        await _telescopeInteractionController.ToggleTelescopeAsync(true);
        _trackableTargetIndicatorManager.SwitchTrackingCameraWithID("Telescope");

        _dropTelescopeButton.SetActive(true);

        _currentSpawnPrefabBridge.GazeTarget_Telescope.ToggleInteractable(false);
        _currentSpawnPrefabBridge.ToggleTelescopeSelectionUI(false);

        PlayScreenDialgueTextWithText("可以自由觀察"); // 不自動關閉

        _arGazeInteractionManager.ToggleGazeInteraction(true);

        _currentSpawnPrefabBridge.GazeTarget_Castle.ToggleInteractable(true);
        _currentSpawnPrefabBridge.TrackableTarget_Castle.ToggleTargetTrackable(true);

        _currentSpawnPrefabBridge.GazeTarget_Gate.ToggleInteractable(true);
        _currentSpawnPrefabBridge.TrackableTarget_Gate.ToggleTargetTrackable(true);

        _currentSpawnPrefabBridge.GazeTarget_CastleWorker.ToggleInteractable(true);
        _currentSpawnPrefabBridge.TrackableTarget_CastleWorker.ToggleTargetTrackable(true);

        // 等待玩家注視熱蘭遮城或鹿耳門，自由觀察
    }

    private async void OnSelectInTelescopeView_Castle_EndGame()
    {
        // 在撥放熱蘭遮城對話時，暫時關閉其他互動與追蹤
        // 但結束後重啟

        _dropTelescopeButton.SetActive(false);

        _arGazeInteractionManager.ToggleGazeInteraction(false);

        _currentSpawnPrefabBridge.TrackableTarget_Gate.ToggleTargetTrackable(false);
        _currentSpawnPrefabBridge.TrackableTarget_CastleWorker.ToggleTargetTrackable(false);

        await PlayScreenDialogueTextWithScriptAsync(_scenario_Telescope_Castle);

        _dropTelescopeButton.SetActive(true);

        _arGazeInteractionManager.ToggleGazeInteraction(true);

        _currentSpawnPrefabBridge.TrackableTarget_Gate.ToggleTargetTrackable(true);
        _currentSpawnPrefabBridge.TrackableTarget_CastleWorker.ToggleTargetTrackable(true);

        PlayScreenDialgueTextWithText("可以自由觀察");
    }

    private async void OnSelectInTelescopeView_Gate_EndGame()
    {
        // 在撥放鹿耳門對話時，暫時關閉其他互動與追蹤
        // 但結束後重啟

        _dropTelescopeButton.SetActive(false);

        _arGazeInteractionManager.ToggleGazeInteraction(false);

        _currentSpawnPrefabBridge.TrackableTarget_Castle.ToggleTargetTrackable(false);
        _currentSpawnPrefabBridge.TrackableTarget_CastleWorker.ToggleTargetTrackable(false);

        await PlayScreenDialogueTextWithScriptAsync(_scenario_Telescope_Gate);

        _dropTelescopeButton.SetActive(true);

        _arGazeInteractionManager.ToggleGazeInteraction(true);

        _currentSpawnPrefabBridge.TrackableTarget_Castle.ToggleTargetTrackable(true);
        _currentSpawnPrefabBridge.TrackableTarget_CastleWorker.ToggleTargetTrackable(true);

        PlayScreenDialgueTextWithText("可以自由觀察");
    }

    public async void OnSelectInTelescopeView_CastleWorker_EndGame()
    {
        // 在撥放修城工人對話時，暫時關閉其他互動與追蹤
        // 但結束後重啟

        _dropTelescopeButton.SetActive(false);

        _arGazeInteractionManager.ToggleGazeInteraction(false);

        _currentSpawnPrefabBridge.TrackableTarget_Castle.ToggleTargetTrackable(false);
        _currentSpawnPrefabBridge.TrackableTarget_Gate.ToggleTargetTrackable(false);

        await PlayScreenDialogueTextWithScriptAsync(_scenario_CastleWorker);

        _dropTelescopeButton.SetActive(true);

        _arGazeInteractionManager.ToggleGazeInteraction(true);

        _currentSpawnPrefabBridge.TrackableTarget_Castle.ToggleTargetTrackable(true);
        _currentSpawnPrefabBridge.TrackableTarget_Gate.ToggleTargetTrackable(true);

        PlayScreenDialgueTextWithText("可以自由觀察");
    }

    public async void DropTelescopeInEndGame()
    {
        _dropTelescopeButton.SetActive(false);

        _currentSpawnPrefabBridge.GazeTarget_Castle.ToggleInteractable(false);
        _currentSpawnPrefabBridge.TrackableTarget_Castle.ToggleTargetTrackable(false);
        _currentSpawnPrefabBridge.GazeTarget_Gate.ToggleInteractable(false);
        _currentSpawnPrefabBridge.TrackableTarget_Gate.ToggleTargetTrackable(false);
        _currentSpawnPrefabBridge.GazeTarget_CastleWorker.ToggleInteractable(false);
        _currentSpawnPrefabBridge.TrackableTarget_CastleWorker.ToggleTargetTrackable(false);

        await _telescopeInteractionController.ToggleTelescopeAsync(false);
        _trackableTargetIndicatorManager.SwitchTrackingCameraWithID("Main");

        EndGameFlow();
    }









    // ===========================================
    // 以下為一些通用方法：

    // ==========================================
    // 播放對話文字（螢幕文字盒）
    CancellationTokenSource _playScreenDialogueTextCts;
    private async Task PlayScreenDialogueTextWithScriptAsync(SimpleDialogueAnimationScript_SO script)
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

    private async Task PlayScreenDialgueTextWithTextAsync(string text, float waitDuration)
    {
        _screenDialogueTextBoxController.ToggleDisplay(true);
        await _screenDialogueTextBoxController.SetTextWithTypewriterEffectAsync(text);
        await Task.Delay((int)(waitDuration * 1000));
        _screenDialogueTextBoxController.ToggleDisplay(false);
    }

    private async void PlayScreenDialgueTextWithText(string text) // 不自動關閉
    {
        _screenDialogueTextBoxController.ToggleDisplay(true);
        await _screenDialogueTextBoxController.SetTextWithTypewriterEffectAsync(text);
        // 需要手動關閉！
    }

    private void DisableScreenDialogueText()
    {
        _screenDialogueTextBoxController.ToggleDisplay(false);
    }

    // ===========================================
    // 由此來編寫玩家視線觸發事件，以ID作為觸發區分
    private void HandleGazeTrigger(string targetID, ARGazeTarget target)
    {
        DebugMessageManager.Instance.AddMessage($"GameManager Received Trigger: {targetID}");

        switch (targetID) {

            case "LW":
                
                StartScenario1();
                _currentSpawnPrefabBridge.GazeTarget_LW.ToggleInteractable(false);
                break;

            case "ShopKeeper":
                StartScenario2();
                _currentSpawnPrefabBridge.GazeTarget_ShopKeeper.ToggleInteractable(false);
                break;

            case "Telescope":

                if ( !_isMainFlowEnded )
                    OnEnterTelescopeView_MainFlow();
                else
                    OnEnterTelescopeView_EndGame();
                break;

            case "Castle":

                if ( !_isMainFlowEnded )
                    OnSelectInTelescopeView_Castle();
                else
                    OnSelectInTelescopeView_Castle_EndGame();
                break;

            case "Gate":

                if ( !_isMainFlowEnded )
                    OnSelectInTelescopeView_Gate();
                else
                    OnSelectInTelescopeView_Gate_EndGame();
                break;

            case "CastleWorker":

                OnSelectInTelescopeView_CastleWorker_EndGame();
                break;

            case "Gabezo_A":

                OnSelect_Gabezo_A_EndGame();
                break;

            case "Gabezo_B":

                OnSelect_Gabezo_B_EndGame();
                break;

            default:
                DebugMessageManager.Instance.AddMessage($"收到未知的 ID: {targetID}", true);
                return;
        }
    }

    // ==========================================
    // 進出 Portal 呼叫事件
    public void Event_PlayerEnterPortal()
    {
        DebugMessageManager.Instance.AddMessage("Player Enter Portal World");
        /*_cameraObjectInPortalDisplayMaskObj.SetActive(true);
        DebugMessageManager.Instance.AddMessage("Show portal display mask");*/

        _isPlayerInPortalWorld = true;
    }

    public void Event_PlayerExitPortal()
    {
        DebugMessageManager.Instance.AddMessage("Player Exit Portal World");
        /*_cameraObjectInPortalDisplayMaskObj.SetActive(false);
        DebugMessageManager.Instance.AddMessage("Hide portal display mask");*/

        _isPlayerInPortalWorld = false;
    }

    public void Event_DropTelescopeInEndGame()
    {
        DropTelescopeInEndGame();
    }

}
