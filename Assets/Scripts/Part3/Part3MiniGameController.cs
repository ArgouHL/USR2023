using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class Part3MiniGameController : MonoBehaviour
{
    [SerializeField] private SimpleCharacterControllerPairManager _characterAndTextBoxControllerPairManager;
    [SerializeField] SimpleDialogueAnimationScript_SO cargobox_dialogue_1;
    [SerializeField] SimpleDialogueAnimationScript_SO cargobox_dialogue_2;
    [SerializeField] SimpleDialogueAnimationScript_SO cargobox_dialogue_3;
    [SerializeField] DissolveController cargobox_dissolve_1;
    [SerializeField] DissolveController cargobox_dissolve_2;
    [SerializeField] DissolveController cargobox_dissolve_3;

    [SerializeField] GameObject ClickItems;

    bool inDialog = false;

    int progress = 0;

    public void Initialize(SimpleCharacterControllerPairManager manager)
    {
        _characterAndTextBoxControllerPairManager = manager;
        ClickItems.SetActive(false);
    }

    public async Task<bool> ClickItem(int index)
    {
        if(inDialog) return false;
        inDialog = true;
        switch (index)
        {
            case 1:
                DebugMessageManager.Instance.AddMessage("第一組路人對話");
                await _characterAndTextBoxControllerPairManager.PlayScriptAsync(cargobox_dialogue_1); 
                await cargobox_dissolve_1.ToggleEnableWithDissolveEffectAsync(false);
                inDialog = false;
                progress++;
                return true;
            case 2:
                DebugMessageManager.Instance.AddMessage("第二組路人對話");
                await _characterAndTextBoxControllerPairManager.PlayScriptAsync(cargobox_dialogue_2);
                await cargobox_dissolve_2.ToggleEnableWithDissolveEffectAsync(false);
                inDialog = false;
                progress++;
                return true;
            case 3:
                DebugMessageManager.Instance.AddMessage("第三組路人對話");
                await _characterAndTextBoxControllerPairManager.PlayScriptAsync(cargobox_dialogue_3);
                await cargobox_dissolve_3.ToggleEnableWithDissolveEffectAsync(false);
                inDialog = false;
                progress++;
                return true;
        }
        return true;
    }

    public async Task WaitGameOver()
    {
        ClickItems.SetActive(true);
        while (progress < 3)
        {
            await Task.Yield();
        }
    }
}
