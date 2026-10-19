using UnityEngine;

public class SetFadeCenter : MonoBehaviour
{
    [SerializeField]
    Material[] fadeMaterials;

    [SerializeField] float fadeDistance = 120f;
    void Start()
    {
        foreach (var m in fadeMaterials)
        {
           m.SetVector("_Center", transform.position);
            m.SetFloat("_fadeDistance", fadeDistance);
        }

    }

    
}
