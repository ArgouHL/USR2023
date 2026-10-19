using UnityEngine;

public class Part2MiniGameItem :  MonoBehaviour, IClickable
{
    [SerializeField] Part2MiniGameController miniGameController;
    [SerializeField] Part2MiniGameController.Item item;

    [SerializeField] Animator animator;
    
    public async void OnClicked(RaycastHit hit)
    {
        //if (await miniGameController.ClickItem(item))
        await miniGameController.ClickItem(item);
    }
}

