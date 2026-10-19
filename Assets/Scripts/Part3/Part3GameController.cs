using System.Threading;
using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;

public class Part3GameController : MonoBehaviour
{
    [SerializeField] private SimpleCharacterControllerPairManager _characterAndTextBoxControllerPairManager;
    [SerializeField] private SimpleCharacterContainer _characterObjContainer;
    [SerializeField] ARGazeInteractionManager _arGazeInteractionManager;
    Coroutine _GameCoroutine;
    [SerializeField] Part3MiniGameController miniGameController;
    [SerializeField] PortalController_v2 portalController;
    [SerializeField] FootStepControl footStepControl;
    [SerializeField] Animator portalAnimator;
    [SerializeField] List<SimpleTrackableTarget> targets = new(); // 為了初始化 可掃描提示
    List<string> _tracked = new(); // 紀錄已追蹤的物件

    [Header("對話")]
    [SerializeField] SimpleDialogueAnimationScript_SO opening;
    [SerializeField] SimpleDialogueAnimationScript_SO wlak_in_portal;
    [SerializeField] SimpleDialogueAnimationScript_SO assistant_run_in_scene;
    [SerializeField] SimpleDialogueAnimationScript_SO assistant_run_to_opposite_side;

    [Header("ARGazeTarget")]
    [SerializeField] ARGazeTarget openingMascot_ARGazeTarget;

    [Header("TrackableTarget")]
    [SerializeField] SimpleTrackableTarget openingMascot_TrackableTarget;

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
        miniGameController.Initialize(_characterAndTextBoxControllerPairManager);
    }
    private void HandleGazeTrigger(string targetID, ARGazeTarget target)
    {
        DebugMessageManager.Instance.AddMessage($"GameManager Received Trigger: {targetID}");

        _tracked.Add(targetID);
    }

    void Start()
    {
        DebugMessageManager.Instance.AddMessage("Part3GameController Start");
        Game();
    }

    async void Game()
    {
        // 開啟西門寶寶的掃描
        _arGazeInteractionManager.ToggleGazeInteraction(true);
        openingMascot_ARGazeTarget.ToggleInteractable(true);
        openingMascot_TrackableTarget.ToggleTargetTrackable(true);
        SceneContext.Instance.PlayScreenDialgueTextWithText("請將攝影機看向我~");

        // 顯示西門寶寶
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("mascot_opening", true);
        
        // 當看到西門寶寶
        while(!_tracked.Contains("mascot_opening")) await Task.Yield();
        _arGazeInteractionManager.ToggleGazeInteraction(false);
        openingMascot_ARGazeTarget.ToggleInteractable(false);
        openingMascot_TrackableTarget.ToggleTargetTrackable(false);
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

        // 開啟傳送門
        portalAnimator.Play("Portal_OpenDoor");
        portalAnimator.Update(0);
        waitTime = portalAnimator.GetCurrentAnimatorStateInfo(0).length;
        await portalController.ShowAsync();
        //await Task.Delay((int)(waitTime * 1000));

        // 走入傳送門
        await _characterAndTextBoxControllerPairManager.PlayScriptAsync(wlak_in_portal);

        SceneContext.Instance.PlayScreenDialgueTextWithText("請進入時空門");
        while (!playerInPortal)
        {
            await Task.Yield();
        }
        SceneContext.Instance.DisableScreenDialogueText();

        // 開場
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("assistant", true);
        await _characterAndTextBoxControllerPairManager.PlayScriptAsync(assistant_run_in_scene); 

        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("mob_1", true);
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("mob_2", true);
        _characterAndTextBoxControllerPairManager.ToggleCharacterDisplay("mob_3", true);

        await miniGameController.WaitGameOver();

        await _characterAndTextBoxControllerPairManager.PlayScriptAsync(assistant_run_to_opposite_side); 
        
        SceneContext.Instance.PlayScreenDialgueTextWithText("可以自由觀察");
    }

    public void PlayerInPortalToggle(bool v)
    {
        playerInPortal = v;
    }

}
