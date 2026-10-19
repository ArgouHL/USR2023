using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
using System;
using UnityEngine.UIElements;

public class DialogueCharacterController : MonoBehaviour
{
    public DialogueTextBoxUIController DialogueTextBoxUIController;

    [SerializeField]
    private Transform _dialogueTextBoxUITargetPointTrans;
    public Transform DialogueTextBoxUITargetPointTrans => _dialogueTextBoxUITargetPointTrans;

    [SerializeField]
    private Animator _characterAnimator, // 角色整體物件的動畫機
        _characterSubAnimator; // 角色動作的動畫機

    [SerializeField]
    private Renderer[] _graphxRenderer;

    public void Initialize()
    {
        DialogueTextBoxUIController.Initialize();
        ToggleEnable(false);
    }

    public void ToggleEnable(bool isEnable)
    {
       gameObject.SetActive(isEnable);
    }

    private bool _isInTransition = false;
    public async Task ToggleEnableWithDissolveEffectAsync(bool isEnable)
    {
        if (_isInTransition) return;
        _isInTransition = true;

        if ( isEnable )
        {
            float value = 1;
            while (value > 0) {
                foreach (var renderer in _graphxRenderer){
                    renderer.material.SetFloat("_DisssolveAmount", value);
                }
                await Task.Delay(TimeSpan.FromSeconds(0.05f));
                value -= 0.02f;
            }
            foreach (var renderer in _graphxRenderer){
                renderer.material.SetFloat("_DisssolveAmount", 0);
            }

        }
        else {
            float value = 0;
            while (value < 1){
                foreach (var renderer in _graphxRenderer)
                {
                    renderer.material.SetFloat("_DisssolveAmount", value);
                }
                await Task.Delay(TimeSpan.FromSeconds(0.05f));
                value += 0.02f;
            }
            foreach (var renderer in _graphxRenderer)
            {
                renderer.material.SetFloat("_DisssolveAmount", 1);
            }
        }

        _isInTransition = false;
    }

    public async void ToggleEnableWithDissolveEffect(bool isEnable)
    {
        if (_isInTransition) return;
        _isInTransition = true;

        if (isEnable)
        {
            float value = 1;
            while (value > 0)
            {
                foreach (var renderer in _graphxRenderer)
                {
                    renderer.material.SetFloat("_DisssolveAmount", value);
                }
                await Task.Delay(TimeSpan.FromSeconds(0.05f));
                value -= 0.02f;
            }
            foreach (var renderer in _graphxRenderer)
            {
                renderer.material.SetFloat("_DisssolveAmount", 0);
            }

        }
        else
        {
            float value = 0;
            while (value < 1)
            {
                foreach (var renderer in _graphxRenderer)
                {
                    renderer.material.SetFloat("_DisssolveAmount", value);
                }
                await Task.Delay(TimeSpan.FromSeconds(0.05f));
                value += 0.02f;
            }
            foreach (var renderer in _graphxRenderer)
            {
                renderer.material.SetFloat("_DisssolveAmount", 1);
            }
        }

        _isInTransition = false;
    }

    public void PlayAnimationClipByName(string clipName)
    {
        _characterAnimator.Play(clipName);
    }

    public IEnumerator PlayAnimationClipByNameCor(string clipName, TaskCompletionSource<bool> tcs)
    {
        _characterAnimator.Play(clipName);
        float waitTime;
        _characterAnimator.Update(0);
        var stateInfo = _characterAnimator.GetCurrentAnimatorStateInfo(0);
        waitTime = stateInfo.length;

        if (!stateInfo.IsName(clipName)) DebugMessageManager.Instance.AddMessage($"The animation currently played {clipName} doesn't match!", true);

        DebugMessageManager.Instance.AddMessage($"Play Animation {clipName}. Until animation end, which cost {waitTime} second.");

        yield return new WaitForSeconds(waitTime);
        tcs.SetResult(true);

        DebugMessageManager.Instance.AddMessage($"The animation {clipName} finished. task completation marked.");

        // 需注意使用Task不會因為物件被摧毀就消失，不過在這裡理論上不會出現這種事情
    }

    public void PlaySubAnimatorAnimationClipByName(string clipName) // assume will be called by animation event.
    {
        _characterSubAnimator.Play(clipName);
    }
}
