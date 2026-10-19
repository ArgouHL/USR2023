using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InteractionControllerBase : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    [SerializeField]
    private UnityEvent OnClickInteractionEvent,
        OnDownInteractionEvent;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickInteractionEvent?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDownInteractionEvent?.Invoke();
    }
}
