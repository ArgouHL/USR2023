using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
using System;

public class DissolveController : MonoBehaviour
{
    [SerializeField] private Material[] materials;
    private bool _isInTransition = false;
    float pre_value;
    public bool Default = false;
    public float duration = 1;

    void Awake()
    {
        if (Default)
        {
            foreach (var material in materials){
                material.SetFloat("_DisssolveAmount", 0);
            }
        }
        else
        {
            foreach (var material in materials){
                material.SetFloat("_DisssolveAmount", 1);
            }
        }

        pre_value = 1 / (duration / 0.05f);
    }
    public async Task ToggleEnableWithDissolveEffectAsync(bool isEnable)
    {
        if (_isInTransition) return;
        _isInTransition = true;

        if ( isEnable )
        {
            float value = 1;
            while (value > 0) {
                foreach (var material in materials){
                    material.SetFloat("_DisssolveAmount", value);
                }
                await Task.Delay(TimeSpan.FromSeconds(0.05f));
                value -= pre_value;
            }
            foreach (var material in materials){
                material.SetFloat("_DisssolveAmount", 0);
            }

        }
        else {
            float value = 0;
            while (value < 1){
                foreach (var material in materials)
                {
                    material.SetFloat("_DisssolveAmount", value);
                }
                await Task.Delay(TimeSpan.FromSeconds(0.05f));
                value += pre_value;
            }
            foreach (var material in materials)
            {
                material.SetFloat("_DisssolveAmount", 1);
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
                foreach (var material in materials)
                {
                    material.SetFloat("_DisssolveAmount", value);
                }
                await Task.Delay(TimeSpan.FromSeconds(0.05f));
                value -= 0.02f;
            }
            foreach (var material in materials)
            {
                material.SetFloat("_DisssolveAmount", 0);
            }

        }
        else
        {
            float value = 0;
            while (value < 1)
            {
                foreach (var material in materials)
                {
                    material.SetFloat("_DisssolveAmount", value);
                }
                await Task.Delay(TimeSpan.FromSeconds(0.05f));
                value += 0.02f;
            }
            foreach (var material in materials)
            {
                material.SetFloat("_DisssolveAmount", 1);
            }
        }

        _isInTransition = false;
    }
}
