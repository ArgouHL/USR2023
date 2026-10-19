using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Part2MiniGameController: MonoBehaviour
{
    [SerializeField] GameObject MiniGamePrefab;
    [Serializable]
    public enum Item
    {
        Brick, Battlement, Wood
    }
    
    int progress_index;
    List<Item> GameAnswer = new()
    {
        Item.Brick,
        Item.Battlement,
        Item.Wood
    };

    public TowerAniCtr towerAniCtr;

    SimpleCharacterControllerPairManager _characterAndTextBoxControllerPairManager;

    [Header("對話")]
    [SerializeField] Animator BrickAnimator;
    [SerializeField] Animator BattlementAnimator;
    [SerializeField] Animator WoodAnimator;

    [Header("對話")]
    [SerializeField] SimpleDialogueAnimationScript_SO Brick_Brick;
    [SerializeField] SimpleDialogueAnimationScript_SO Brick_Battlement;
    [SerializeField] SimpleDialogueAnimationScript_SO Brick_Wood;
    [SerializeField] SimpleDialogueAnimationScript_SO Battlement_Battlement;
    [SerializeField] SimpleDialogueAnimationScript_SO Battlement_Wood;
    [SerializeField] SimpleDialogueAnimationScript_SO Wood_Wood;

    bool inDialog;


    void Awake()
    {
        MiniGamePrefab.SetActive(false);
    }

    public void Initialized(SimpleCharacterControllerPairManager manager)
    {
        _characterAndTextBoxControllerPairManager = manager;
        inDialog = true;
    }

    public async Task ClickItem(Item item)
    {
        if(inDialog) return;
        inDialog = true;

        if (GameAnswer[progress_index] != item)
        {
            // 錯誤答案
            Debug.Log("錯誤");
            DebugMessageManager.Instance.AddMessage("錯誤");
            await _characterAndTextBoxControllerPairManager.PlayScriptAsync(getMascotDialog(item));
            inDialog = false;
            return;
        }
        // 正確答案
        Debug.Log("正確");
        DebugMessageManager.Instance.AddMessage("正確");
        SetTowerProgress(item);
        await _characterAndTextBoxControllerPairManager.PlayScriptAsync(getMascotDialog(item));
        progress_index += 1;
        inDialog = false;
        return;
    }

    public bool isGameOver()
    {
        if (progress_index >= GameAnswer.Count)
        {
            // 遊戲結束
            return true;
        }
        // 遊戲尚未結束
        return false;
    }

    public void SetGame()
    {
        MiniGamePrefab.SetActive(true);
    }
    public void StartGame()
    {
        inDialog = false;
    }

    SimpleDialogueAnimationScript_SO getMascotDialog(Item anwser_item)
    {
        if (GameAnswer[progress_index] == Item.Brick) {
            if (anwser_item == Item.Brick) {
                return Brick_Brick;
            } else if (anwser_item == Item.Battlement) {
                return Brick_Battlement;
            } else if (anwser_item == Item.Wood) {
                return Brick_Wood;
            }
        } else if (GameAnswer[progress_index] == Item.Battlement) {
            if (anwser_item == Item.Battlement) {
                return Battlement_Battlement;
            } else if (anwser_item == Item.Wood) {
                return Battlement_Wood;
            } 
        } else if (GameAnswer[progress_index] == Item.Wood) {
                return Wood_Wood;
        }
        return null;
    }

    
    void SetTowerProgress(Item item)
    {
        switch (item)
        {
            case Item.Brick:
                towerAniCtr.StartTowerAniOne();
                BrickAnimator.Play("FlyToTower");
                return;
            case Item.Battlement:
                towerAniCtr.StartTowerAniTwo();
                BattlementAnimator.Play("FlyToTower");
                return;
            case Item.Wood:
                towerAniCtr.StartTowerAniThree();
                WoodAnimator.Play("FlyToTower");
                return;
        }
    }


}
