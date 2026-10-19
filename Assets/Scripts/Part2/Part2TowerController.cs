using UnityEngine;

public class Part2TowerController : MonoBehaviour
{
    [SerializeField] GameObject Brick, 
    Battlement, 
    Wood;

    void Awake()
    {
        Brick.SetActive(false);
        Battlement.SetActive(false);
        Wood.SetActive(false);
    }
    public void SetTowerProgress(Part2MiniGameController.Item item)
    {
        switch (item)
        {
            case Part2MiniGameController.Item.Brick:
                Brick.SetActive(true);
                return;
            case Part2MiniGameController.Item.Battlement:
                Brick.SetActive(true);
                Battlement.SetActive(true);
                return;
            case Part2MiniGameController.Item.Wood:
                Brick.SetActive(true);
                Battlement.SetActive(true);
                Wood.SetActive(true);
                return;
        }
    }
}
