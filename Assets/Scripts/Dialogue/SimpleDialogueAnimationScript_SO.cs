using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class SimpleDialogueAnimationData
{
    public string CharacterName = "";

    [TextArea]
    public string Line = "";
    public float WaitDurationAfterLine = 1f;

    public string AnimationClipName = "";
    public bool IsWaitUntilAnimationEnd = false;

    public string SubAnimationClipName = ""; // 是給不想只為了一個SubAnimationClip而開一個新的Animation用的，所以最好不要和上面的AnimationClip一起使用
}

[CreateAssetMenu(fileName = "SimpleDialogueAnimationScript_SO", menuName = "ScriptableObjects/SimpleDialogueAnimationScript_SO")]
public class SimpleDialogueAnimationScript_SO : ScriptableObject
{
    public List<SimpleDialogueAnimationData> ScriptDataList;
}
