using UnityEngine;
using UnityEngine.Events;

public class Part3MiniGameItem :  MonoBehaviour, IClickable
{
    [SerializeField] Part3MiniGameController miniGameController;
    [SerializeField] int itemNumber;
    [SerializeField] UnityEvent unityEvent;

    public async void OnClicked(RaycastHit hit)
    {
        bool result = await miniGameController.ClickItem(itemNumber);
        if (result)
        {
            gameObject.SetActive(false);
            unityEvent?.Invoke();
        }
    }

}

