using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Unity.VisualScripting;

[Serializable]
public class CharacterObjNamePair
{
    public string CharacterName;
    public GameObject CharacterGameObject;
}

public class SimpleCharacterControllerPairManager : MonoBehaviour
{
    private Dictionary<string, DialogueCharacterController>
        _characterAndTextBoxControllerByNameDict = new();

    [SerializeField]
    private Transform _dialogueTextBoxUISpawnTrans;
    [SerializeField]
    private GameObject _dialogueTextBoxUIPrefab;

    // private Coroutine _syncTextBoxPosCoroutine;

    public void Initialize()
    {
        // _syncTextBoxPosCoroutine = StartCoroutine(TrackCharacterPositionSyncTextBoxPosition());
    }

    public void SetupCharacterControllerPair(List<CharacterObjNamePair> objNamePairs)
    {
        foreach (var pair in objNamePairs) {

            if (_characterAndTextBoxControllerByNameDict.ContainsKey(pair.CharacterName)) {
                // already exists
                continue;
            }

            var charObj = pair.CharacterGameObject;
            var charCon = charObj.GetComponent<DialogueCharacterController>();

            /*var textBoxUIObj = Instantiate(_dialogueTextBoxUIPrefab, _dialogueTextBoxUISpawnTrans);
            var textBoxUICon = textBoxUIObj.GetComponent<DialogueTextBoxUIController>();*/

            _characterAndTextBoxControllerByNameDict.Add(
                pair.CharacterName,
                (charCon)
            );

            charCon.Initialize();
        }
    }

    // 同步角色位置到對話框UI位置，但目前已改用全3D畫布顯示
    /*private IEnumerator TrackCharacterPositionSyncTextBoxPosition()
    {
        while (true) {

            foreach (var pair in _characterAndTextBoxControllerByNameDict) {

                if (pair.Value.Item2.IsDisplayActive == false) continue;

                Vector3 screenPos = Camera.main.WorldToScreenPoint
                (pair.Value.Item1.DialogueTextBoxUITargetPointTrans.position);
                // transfer screen point to UI local point

                pair.Value.Item2.SetPositionByScreenPoint(screenPos);
            }
            yield return null;
        }
    }*/

    public async Task PlayScriptAsync(SimpleDialogueAnimationScript_SO script)
    {
        for (int index = 0; index < script.ScriptDataList.Count; index++) {

            var data = script.ScriptDataList[index];

            DialogueCharacterController characterCon =
                _characterAndTextBoxControllerByNameDict[data.CharacterName];
            DialogueTextBoxUIController textBoxCon = characterCon.DialogueTextBoxUIController;

            if (!string.IsNullOrEmpty(data.Line)) {
                textBoxCon.ToggleDisplay(true);
            }

            if (!string.IsNullOrEmpty(data.SubAnimationClipName)){
                characterCon.PlaySubAnimatorAnimationClipByName(data.SubAnimationClipName);
                // 是給不想只為了一個SubAnimationClip而開一個新的Animation用的，所以最好不要和AnimationClip一起使用
            }

            var tcs = new TaskCompletionSource<bool>(); // 只會用在 IsWaitUntilAnimationEnd == true 時被使用
            // 目前因為不會有中斷的情況，所以基本上確保都會被執行完，所以就不多執行cancellation check。
            // 但如果之後還有更多功能要加，建議直接使用UniTask 這邊的都算是便宜行事

            if (characterCon != null && !string.IsNullOrEmpty(data.AnimationClipName)) {
                if ( data.IsWaitUntilAnimationEnd){
                    StartCoroutine(characterCon.PlayAnimationClipByNameCor(data.AnimationClipName, tcs));
                }
                else{
                    characterCon.PlayAnimationClipByName(data.AnimationClipName);
                }
            }

            await textBoxCon.SetTextWithTypewriterEffectAsync(data.Line);
            await Task.Delay((int)(data.WaitDurationAfterLine * 1000));

            if ( data.IsWaitUntilAnimationEnd){
                await tcs.Task;
            }

            //如果下一個data是同一個角色
            if (index + 1 < script.ScriptDataList.Count) {
                var nextData = script.ScriptDataList[index + 1];
                if ( nextData.CharacterName == data.CharacterName &&
                    !string.IsNullOrEmpty(nextData.Line) ) { // 並且有台詞，才不關閉對話視窗UI，否則仍得要關閉
                    continue;
                }
            }

            textBoxCon.ToggleDisplay(false);
        }
    }

    public void ToggleCharacterDisplay(string name, bool isEnable)
    {
        if (!_characterAndTextBoxControllerByNameDict.ContainsKey(name)) {
            Debug.LogWarning($"No character found with name: {name}");
            return;
        }

        _characterAndTextBoxControllerByNameDict[name].ToggleEnable(isEnable);
    }

    public async Task ToggleCharacterDisplayWithDissolveEffectAsync(string name, bool isEnable)
    {
        if (!_characterAndTextBoxControllerByNameDict.ContainsKey(name)) {
            Debug.LogWarning($"No character found with name: {name}");
            return;
        }

        await _characterAndTextBoxControllerByNameDict[name].ToggleEnableWithDissolveEffectAsync(isEnable);
    }

    public async void ToggleCharacterDisplayWithDissolveEffect(string name, bool isEnable)
    {
        if (!_characterAndTextBoxControllerByNameDict.ContainsKey(name))
        {
            Debug.LogWarning($"No character found with name: {name}");
            return;
        }

        _characterAndTextBoxControllerByNameDict[name].ToggleEnableWithDissolveEffect(isEnable);
    }

    public DialogueCharacterController GetCharacterControllerByName(string name) =>
        _characterAndTextBoxControllerByNameDict[name];
}
