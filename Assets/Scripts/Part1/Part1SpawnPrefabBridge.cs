using UnityEngine;

//
// 此腳本會附在生成的Prefab之上，用來橋接Part1GameManager，並執行一些客製化的功能
// 與其他較為通用的但同樣附在Prefab上的腳本類似（e.g. SimpleCharacterContainer...），但此腳本為專屬於Part1的功能。
public class Part1SpawnPrefabBridge : MonoBehaviour
{
    private Part1GameManager _part1GameManager;

    [SerializeField]
    private GameObject _telescopeSelectionUIObj;

    [SerializeField]
    private Animator _waterLevelAnimator;
    [SerializeField]
    private ShipAniCtr _shipAnimController;

    [SerializeField]
    private FootStep _footStepController_1, // 看向小西門
        _footStepController_2; // 走進時空門
    public FootStep FootStepController_1 => _footStepController_1;
    public FootStep FootStepController_2 => _footStepController_2;

    [Header("Portal Control")]
    [SerializeField]
    private GameObject _portalObject;
    public GameObject PortalObject => _portalObject;
    [SerializeField]
    private Animator _portalFrameAnimator;
    public Animator PortalFrameAnimator => _portalFrameAnimator;

    [Header("AR Gaze Targets")]
    [SerializeField]
    private ARGazeTarget _gazeTarget_LW; // 小西門
    [SerializeField]
    private ARGazeTarget _gazeTarget_ShopKeeper, // 店小二
        _gazeTarget_Telescope, // 望遠鏡
        _gazeTarget_Castle,    // 熱蘭遮城
        _gazeTarget_Gate,      // 鹿耳門
        _gazeTarget_CastleWorker, // 修城工人
        _gazeTarget_Gazebo_A, // 峴石亭
        _gazeTarget_Gazebo_B; // 棠蔭亭

    public ARGazeTarget GazeTarget_LW => _gazeTarget_LW;
    public ARGazeTarget GazeTarget_ShopKeeper => _gazeTarget_ShopKeeper;
    public ARGazeTarget GazeTarget_Telescope => _gazeTarget_Telescope;
    public ARGazeTarget GazeTarget_Castle => _gazeTarget_Castle;
    public ARGazeTarget GazeTarget_Gate => _gazeTarget_Gate;
    public ARGazeTarget GazeTarget_CastleWorker => _gazeTarget_CastleWorker;
    public ARGazeTarget GazeTarget_Gazebo_A => _gazeTarget_Gazebo_A;
    public ARGazeTarget GazeTarget_Gazebo_B => _gazeTarget_Gazebo_B;

    [Header("Trackable Targets")]
    [SerializeField]
    private SimpleTrackableTarget _trackableTarget_Castle; // 熱蘭遮城
    [SerializeField]
    private SimpleTrackableTarget _trackableTarget_Gate, // 鹿耳門
        _trackableTarget_CastleWorker, // 修城工人
        _trackableTarget_Gazebo_A, // 峴石亭
        _trackableTarget_Gazebo_B; // 棠蔭亭
    public SimpleTrackableTarget TrackableTarget_Castle => _trackableTarget_Castle;
    public SimpleTrackableTarget TrackableTarget_Gate => _trackableTarget_Gate;
    public SimpleTrackableTarget TrackableTarget_CastleWorker => _trackableTarget_CastleWorker;
    public SimpleTrackableTarget TrackableTarget_Gazebo_A => _trackableTarget_Gazebo_A;
    public SimpleTrackableTarget TrackableTarget_Gazebo_B => _trackableTarget_Gazebo_B;

    public void InitializeAndSetReference(Part1GameManager part1GameManager)
    {
        _part1GameManager = part1GameManager;

        _portalObject.SetActive(false);

    _footStepController_1.gameObject.SetActive(false);
        _footStepController_2.gameObject.SetActive(false);

        ToggleTelescopeSelectionUI(false);

        _gazeTarget_LW.Initialize();
        _gazeTarget_ShopKeeper.Initialize();
        _gazeTarget_Telescope.Initialize();
        _gazeTarget_Castle.Initialize();
        _gazeTarget_Gate.Initialize();
        _gazeTarget_CastleWorker.Initialize();
        _gazeTarget_Gazebo_A.Initialize();
        _gazeTarget_Gazebo_B.Initialize();

        _trackableTarget_Castle.InitializeAndSetReference(_part1GameManager.TrackableTargetIndicatorManager);
        _trackableTarget_Gate.InitializeAndSetReference(_part1GameManager.TrackableTargetIndicatorManager);
        _trackableTarget_CastleWorker.InitializeAndSetReference(_part1GameManager.TrackableTargetIndicatorManager);
        _trackableTarget_Gazebo_A.InitializeAndSetReference(_part1GameManager.TrackableTargetIndicatorManager);
        _trackableTarget_Gazebo_B.InitializeAndSetReference(_part1GameManager.TrackableTargetIndicatorManager);
    }

    public void ToggleTelescopeSelectionUI(bool isEnable)
    {
        _telescopeSelectionUIObj.SetActive(isEnable);
    }

    public void StartWaterLevelRiseAnimation()
    {
        _waterLevelAnimator.Play("Rise");
    }

    public void StartPlayShipAnimation()
    {
        _shipAnimController.StartShipAni();
    }
}
