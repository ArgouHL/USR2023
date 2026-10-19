using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

public class FootStepControl : MonoBehaviour
{
    [SerializeField]
    [Tooltip("是否在開始時自動播放")]
    private bool _startActive = true;

    [SerializeField]
    private bool _disableObjAfterHide = false;

    public Material footVFX;
    public float AmountStart = 1;
    public float AmountEnd = 1;

    private float _lifeTime = 5f;

    private void Awake()
    {
        footVFX.SetFloat("_AmountStart", 0);
        footVFX.SetFloat("_AmountEnd", AmountEnd);
    }

    private async void Start()
    {
        if (!_startActive) return;

        Show();

        await Task.Delay((int)_lifeTime * 1000);

    }

    public void Show()
    {
        footVFX.SetFloat("_AmountStart", 0);
        footVFX.SetFloat("_AmountEnd", AmountEnd);
        LeanTween.value(0, 1, 2).setOnUpdate((float val)=> footVFX.SetFloat("_AmountStart", val));

    }

    public void Hide()
    {
        LeanTween.value(AmountEnd, 0, 2).setOnUpdate((float val) => footVFX.SetFloat("_AmountEnd", val)).setOnComplete(() =>
        {
            if (_disableObjAfterHide) {
                gameObject.SetActive(false);
            }
        });
    }
}
