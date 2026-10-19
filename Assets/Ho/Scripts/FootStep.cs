using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

public class FootStep : MonoBehaviour
{
    [SerializeField]
    [Tooltip("是否在開始時自動播放")]
    private bool _startActive = true;

    [SerializeField]
    private bool _disableObjAfterHide = false;

    public Material footVFX;

    private float _lifeTime = 5f;

    private void Awake()
    {
        footVFX.SetFloat("_AmountStart", 0);
        footVFX.SetFloat("_AmountEnd", 1);
    }

    private async void Start()
    {
        if (!_startActive) return;

        Show();

        await Task.Delay((int)_lifeTime * 1000);

        Hide();
    }

    public void Show()
    {
        footVFX.SetFloat("_AmountStart", 0);
        footVFX.SetFloat("_AmountEnd", 1);
        LeanTween.value(0, 1, 2).setOnUpdate((float val)=> footVFX.SetFloat("_AmountStart", val));

    }

    public void Hide()
    {
        LeanTween.value(1, 0, 2).setOnUpdate((float val) => footVFX.SetFloat("_AmountEnd", val)).setOnComplete(() =>
        {
            if (_disableObjAfterHide) {
                gameObject.SetActive(false);
            }
        });
    }
}
