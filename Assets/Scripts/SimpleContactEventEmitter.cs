using UnityEngine;
using UnityEngine.Events;

public class SimpleContactEventEmitter : MonoBehaviour
{
    [SerializeField]
    private string _contactTag = "";

    [SerializeField]
    private UnityEvent _onContactEnter,
        _onContactExit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_contactTag)) {
            DebugMessageManager.Instance.AddMessage($"觸發碰撞: {other.name}");

            _onContactEnter?.Invoke();  
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ( other.CompareTag(_contactTag) ) {
            DebugMessageManager.Instance.AddMessage($"結束碰撞: {other.name}");

            _onContactExit?.Invoke();
        }
    }


}
