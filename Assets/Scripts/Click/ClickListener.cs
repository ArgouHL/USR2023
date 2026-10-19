using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class ClickListener : MonoBehaviour
{
    [SerializeField] bool debugLog = false;
    void Update()
    {
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 pos = Touchscreen.current.primaryTouch.position.ReadValue();
            Click(pos);
        }
        else if (Mouse.current != null &&
                 Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 pos = Mouse.current.position.ReadValue();
            Click(pos);
        }
    }

    void Click(Vector2 pos)
    {
        Ray ray = Camera.main.ScreenPointToRay(pos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            hit.collider.GetComponent<IClickable>()?.OnClicked(hit);
            if (debugLog)
            {
                Debug.Log(hit.collider.name);
                DebugMessageManager.Instance.AddMessage(hit.collider.name);
            }
        }
    }

}
