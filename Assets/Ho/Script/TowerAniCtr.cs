using System;
using UnityEngine;

public class TowerAniCtr : MonoBehaviour
{
    [SerializeField] private Material[] partOneMat;
    [SerializeField] private Material[] partTwoMat;
    [SerializeField] private Material[] partThreeMat;

    [SerializeField] private float partOneDuration = 2f;
    [SerializeField] private float partTwoDuration = 2f;
    [SerializeField] private float partThreeDuration = 2f;


    [SerializeField] private ParticleSystem partOnePs;
    [SerializeField] private ParticleSystem partTwoPs;


    [SerializeField] private Animator partThreeAni;
    [SerializeField] private ParticleSystem partThreePs;
 


    private void OnEnable()
    {
     
        float orgY = transform.position.y;
        Debug.Log(orgY);
        for (int i = 0; i < partOneMat.Length; i++)
        {
            partOneMat[i].SetFloat("_FadeTime", 0);
            partOneMat[i].SetFloat("_SpawnYOrigin", orgY);
        }
        for (int j = 0; j < partTwoMat.Length; j++)
        {
            partTwoMat[j].SetFloat("_FadeTime", 0);
            partTwoMat[j].SetFloat("_SpawnYOrigin", orgY);
        }
        for (int k = 0; k < partThreeMat.Length; k++)
        {
            partThreeMat[k].SetFloat("_FadeTime", 0);
            partThreeMat[k].SetFloat("_SpawnYOrigin", orgY);
        }
    }

    [ContextMenu("TestShow")]
    public  void TestShow()
    {
        StartTowerAniOne();
        LeanTween.delayedCall(4f, () => StartTowerAniTwo());
        LeanTween.delayedCall(10f, () => StartTowerAniThree());
    }



    public void StartTowerAniOne()
    {
        partOnePs.Play();
        LeanTween.delayedCall(1f, () =>{
            LeanTween.value(0, 1, partOneDuration).setOnUpdate((float val) =>
            {
                for (int i = 0; i < partOneMat.Length; i++)
                {
                    partOneMat[i].SetFloat("_FadeTime", val);
                }
            } );
        });

    }
    public void StartTowerAniTwo()
    {
        partTwoPs.Play();
        LeanTween.delayedCall(0.5f, () =>
        {
            for (int i = 0; i < partTwoMat.Length; i++)
            {
                int index = i;
                LeanTween.delayedCall(0.5f * index, () =>
                LeanTween.value(0, 1, partTwoDuration).setOnUpdate((float val) =>
                     partTwoMat[index].SetFloat("_FadeTime", val)));

            }
        });
    }
    public void StartTowerAniThree()
    {
        partThreeAni.Play("WoodDrop");
        LeanTween.delayedCall(1.3f, () =>
      partThreePs.Play());
        LeanTween.delayedCall(0.7f, () =>
        {
            LeanTween.value(0, 1, partThreeDuration).setOnUpdate((float val) =>
            {
                for (int i = 0; i < partThreeMat.Length; i++)
                {
                    partThreeMat[i].SetFloat("_FadeTime", val);
                }
            });
        });
    }
}
