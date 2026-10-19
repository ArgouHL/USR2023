using System.Threading;
using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;

public class Part2GameController : MonoBehaviour
{
    [SerializeField] private SimpleCharacterControllerPairManager _characterAndTextBoxControllerPairManager;
    [SerializeField] private SimpleCharacterContainer _characterObjContainer;
    [SerializeField] ARGazeInteractionManager _arGazeInteractionManager;
    Coroutine _GameCoroutine;
    [SerializeField] Part2MiniGameController miniGameController;
    [SerializeField] Animator portalAnimator;
    [SerializeField] GameObject mapUIPrefab;
    [SerializeField] PortalController_v2 portalController;
    [SerializeField] FootStepControl footStepControl;

    


    [SerializeField] List<SimpleTrackableTarget> targets = new(); // 為了初始化 可掃描提示
    List<string> _tracked = new(); // 紀錄已追蹤的物件

    [Header("對話")]
    [SerializeField] SimpleDialogueAnimationScript_SO opening;
    [SerializeField] SimpleDialogueAnimationScript_SO wlak_in_portal;
    [SerializeField] SimpleDialogueAnimationScript_SO game_instructions;
    [SerializeField] SimpleDialogueAnimationScript_SO game_instructions_2;
    [SerializeField] SimpleDialogueAnimationScript_SO ending_1;
    [SerializeField] SimpleDialogueAnimationScript_SO ending_2;

    [Header("ARGazeTarget")]
    [SerializeField] ARGazeTarget openingMascot_ARGazeTarget;
    [SerializeField] ARGazeTarget Gabezo_A_ARGazeTarget;
    [SerializeField] ARGazeTarget Gabezo_B_ARGazeTarget;

    [Header("TrackableTarget")]
    [SerializeField] SimpleTrackableTarget openingMascot_TrackableTarget;
    [SerializeField] SimpleTrackableTarget Gabezo_A_TrackableTarget;
    [SerializeField] SimpleTrackableTarget Gabezo_B_TrackableTarget;

    [Header("Dissolve")]
    [SerializeField] DissolveController assistant_dis;

    bool playerInPortal = false;


    private void Awake()
    {
        _characterAndTextBoxControllerPairManager = SceneContext.Instance.simpleCharacterControllerPairManager;
        _arGazeInteractionManager = SceneContext.Instance.aRGazeInteraction;

        _characterAndTextBoxControllerPairManager.Initialize();

        _characterAndTextBoxControllerPairManager.SetupCharacterControllerPair
            (_characterObjContainer.CharacterObjNamePairs);

        //_arGazeInteractionManager.Initialize();
        _arGazeInteractionManager.OnTargetTriggered += HandleGazeTrigger;

        foreach (var target in targets)
        {
            target.InitializeAndSetReference(SceneContext.Instance.simpleTrackableTargetIndicatorManager);
        }

        portalAnimator.gameObject.SetActive(false);
    }

    void Start()
    {
        DebugMessageManager.Instance.AddMessage("Part2GameController Start");
        miniGameController.Initialized(_characterAndTextBoxControllerPairManager);
        Game();
    }

    async void Game()
    {
        SceneContext.Instance.PlayScreenDialgueTextWithText("請將攝影機看向我~");
        _arGazeInteractionManager.ToggleGazeInteraction(true);

        // 開啟西門寶寶的掃描
        openingMascot_ARGazeTarget.ToggleInteractable(true);

        // 開啟西門寶寶的掃描提示
        openingMascot_TrackableTarget.ToggleTargetTrackable(true);

        // 顯示西門寶寶
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("mascot_opening", true);
        
        
        // 當看到西門寶寶
        while(!_tracked.Contains("mascot_opening")) await Task.Yield();
        openingMascot_ARGazeTarget.ToggleInteractable(false);
        openingMascot_TrackableTarget.ToggleTargetTrackable(false);
        _arGazeInteractionManager.ToggleGazeInteraction(false);
        SceneContext.Instance.DisableScreenDialogueText();
        await Task.Delay(1000);
        footStepControl.Hide();
        
        // 講話
        await _characterAndTextBoxControllerPairManager.PlayScriptAsync(opening);

        // 掉落傳送門
        portalAnimator.gameObject.SetActive(true);
        portalAnimator.Play("Portal_FallFromSky");
        portalAnimator.Update(0);
        float waitTime = portalAnimator.GetCurrentAnimatorStateInfo(0).length;
        await Task.Delay((int)(waitTime * 1000));

        // 開啟場內物件
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("mob_1", true);
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("mob_2", true);
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("mob_3", true);

        // 開啟傳送門
        portalAnimator.Play("Portal_OpenDoor");
        portalAnimator.Update(0);
        waitTime = portalAnimator.GetCurrentAnimatorStateInfo(0).length;
        await portalController.ShowAsync();
        //await Task.Delay((int)(waitTime * 1000));

        // 走入傳送門
        await _characterAndTextBoxControllerPairManager.PlayScriptAsync(wlak_in_portal);

        // 等待玩家進入
        SceneContext.Instance.PlayScreenDialgueTextWithText("請進入時空門");
        while (!playerInPortal)
        {
            await Task.Yield();
        }
        SceneContext.Instance.DisableScreenDialogueText();

        // 遊戲解說
        await _characterAndTextBoxControllerPairManager.PlayScriptAsync(game_instructions);

        // 開始遊戲
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("mob_1", false);
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("mob_2", false);
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("mob_3", false);
        miniGameController.SetGame();
        await _characterAndTextBoxControllerPairManager.PlayScriptAsync(game_instructions_2);
        miniGameController.StartGame();
        while (!miniGameController.isGameOver())
        {
            await Task.Yield();
        }
        
        HeadDownMapUI headDownMapUI = SpawnMap();
        while (!headDownMapUI.looked)
        {
            await Task.Yield();
        }
        await Task.Delay(1000);


        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("assistant", true);

        await _characterAndTextBoxControllerPairManager.PlayScriptAsync(ending_1);
        await assistant_dis.ToggleEnableWithDissolveEffectAsync(true);
        await _characterAndTextBoxControllerPairManager.PlayScriptAsync(ending_2);
        await assistant_dis.ToggleEnableWithDissolveEffectAsync(false);

        _arGazeInteractionManager.ToggleGazeInteraction(true);
        Gabezo_A_ARGazeTarget.ToggleInteractable(true);
        Gabezo_A_TrackableTarget.ToggleTargetTrackable(true);
        Gabezo_B_ARGazeTarget.ToggleInteractable(true);
        Gabezo_B_TrackableTarget.ToggleTargetTrackable(true);


        SceneContext.Instance.PlayScreenDialgueTextWithText("可以自由觀察");
    }

    private void HandleGazeTrigger(string targetID, ARGazeTarget target)
    {
        DebugMessageManager.Instance.AddMessage($"GameManager Received Trigger: {targetID}");

        _tracked.Add(targetID);
        switch (targetID) {
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

    public void PlayerInPortalToggle(bool v)
    {
        playerInPortal = v;
    }

    HeadDownMapUI SpawnMap()
    {
        GameObject mapUI = Instantiate(mapUIPrefab, Camera.main.transform);
        RectTransform mapUIrt = mapUI.GetComponent<RectTransform>();
        mapUIrt.anchorMin = new Vector2(0.5f, 0.5f);
        mapUIrt.anchorMax = new Vector2(0.5f, 0.5f);
        mapUIrt.pivot = new Vector2(0.5f, 0.5f);
        mapUIrt.anchoredPosition = Vector2.zero;
        mapUIrt.localPosition = Vector3.zero;
        HeadDownMapUI headDownMapUI = mapUI.GetComponentInChildren<HeadDownMapUI>();
        return headDownMapUI;
    }


    private async void OnSelect_Gabezo_A_EndGame()
    {
        // 在撥放 峴石亭 對話時，暫時關閉其他互動與追蹤
        // 但結束後重啟

        _arGazeInteractionManager.ToggleGazeInteraction(false);

        Gabezo_A_ARGazeTarget.ToggleInteractable(false);
        Gabezo_B_ARGazeTarget.ToggleInteractable(false);
        Gabezo_B_TrackableTarget.ToggleTargetTrackable(false);

        List<string> dialogs = new List<string>{
            "峴石亭最初為乾隆年間設置於接官亭石坊旁的石亭",
            "在日治時期因地震倒下，並在民國八〇年代重修風神廟與接官亭過程中重新出土",
            "在當時學者在歷史考證的解讀下，被當作成風神廟的鐘樓而重建",
            "後來在2016年的高雄美濃地震毀損，在考古考據後，得知其原先的歷史面貌後重建",
            "碑亭上刻有「峴石」二字，訴說一則百姓感念父母官的歷史故事。"
        };

        foreach (string dialog in dialogs)
        {    
            await SceneContext.Instance.PlayScreenDialgueTextWithTextAsync(dialog, 1);
        }

        _arGazeInteractionManager.ToggleGazeInteraction(true);

        Gabezo_A_ARGazeTarget.ToggleInteractable(true);
        Gabezo_B_ARGazeTarget.ToggleInteractable(true);
        Gabezo_B_TrackableTarget.ToggleTargetTrackable(true);

        SceneContext.Instance.PlayScreenDialgueTextWithText("可以自由觀察");
    }

    private async void OnSelect_Gabezo_B_EndGame()
    {
        // 在撥放 棠蔭亭 對話時，暫時關閉其他互動與追蹤
        // 但結束後重啟

        _arGazeInteractionManager.ToggleGazeInteraction(false);

        Gabezo_A_ARGazeTarget.ToggleInteractable(false);
        Gabezo_B_ARGazeTarget.ToggleInteractable(false);
        Gabezo_A_TrackableTarget.ToggleTargetTrackable(false);

        List<string> dialogs = new List<string>{
            "棠蔭亭最初為乾隆年間設置於接官亭石坊旁的石亭",
            "在日治時期因地震破損，零件被放在南門城附近或被埋入地下",
            "在民國八〇年代重修風神廟與接官亭過程中而重新出土",
            "在當時沒有使用原零件修復，而是被當作成風神廟的鼓樓，仿照一旁竹旳峴石亭（當時的鐘樓）而重建",
            "後來在2016年的高雄美濃地震毀損，在考古考據後，得知其原先的歷史面貌後再次重建",
            "碑亭上刻有「棠蔭」二字，訴說一則百姓感念父母官的歷史故事。"
        };

        foreach (string dialog in dialogs)
        {    
            await SceneContext.Instance.PlayScreenDialgueTextWithTextAsync(dialog, 1);
        }

        _arGazeInteractionManager.ToggleGazeInteraction(true);

        Gabezo_A_ARGazeTarget.ToggleInteractable(false);
        Gabezo_B_ARGazeTarget.ToggleInteractable(false);
        Gabezo_A_TrackableTarget.ToggleTargetTrackable(true);

        SceneContext.Instance.PlayScreenDialgueTextWithText("可以自由觀察");
    }
}
