using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCharacterContainer : MonoBehaviour
{
    [SerializeField]
    private List<CharacterObjNamePair> _characterObjNamePairs;
    public List<CharacterObjNamePair> CharacterObjNamePairs => _characterObjNamePairs;

    /*private Dictionary<string, GameObject>
        _characterObjByNameDict = new();

    public void Initialize() 
    {
        _characterObjByNameDict.Clear();

        foreach (var pair in _characterObjNamePairs) {

            var charObj = pair.CharacterGameObject;

            if (charObj != null) {
                _characterObjByNameDict.Add(
                    pair.CharacterName,
                    charObj
                );
            }
        }
    }

    public GameObject GetCharacterObjByName(string characterName)
    {
        if (_characterObjByNameDict.TryGetValue(characterName, out var characterObj)) {
            return characterObj;
        }
        return null;
    }*/
}
