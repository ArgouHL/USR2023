using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPlayerEnterPortalEventPassable
{
    void Event_PlayerEnterPortal();
    void Event_PlayerExitPortal();
}

public class SimplePortalRelatedController : MonoBehaviour
{
    [SerializeField]
    private PortalController_v2 _portalController;

    private IPlayerEnterPortalEventPassable _gameManagerReference;
    
    public void InitializeAndSetReference(IPlayerEnterPortalEventPassable gameManager)
    {
        _gameManagerReference = gameManager;

        _portalController.gameObject.SetActive(false);
    }

    public async Task TogglePortalAsync(bool enable)
    {
        if ( enable) {
            _portalController.gameObject.SetActive(true);
            await _portalController.ShowAsync();
        }
        else {
            await _portalController.HideAsync();
            _portalController.gameObject.SetActive(false);
        }
    }

    // Event from PortalController, 確認進入世界（非碰撞到傳送門碰撞器）
    public void Event_PlayerEnterPortal() // 玩家進入傳送門世界
    {
        _gameManagerReference.Event_PlayerEnterPortal();
    }

    public void Event_PlayerExitPortal() // 玩家離開傳送門世界（回到原本）
    {
        _gameManagerReference.Event_PlayerExitPortal();
    }
}
