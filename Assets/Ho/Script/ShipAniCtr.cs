using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class ShipAniCtr : MonoBehaviour
{
   [SerializeField] BoatControl[] boatControls;
    [SerializeField] float delayTime = 3f;

    [ContextMenu("StartShipAni")]
    public void StartShipAni()
    {
        StartCoroutine(ShipAni());
       
       
    }

    IEnumerator ShipAni()       
    {
        while (true)
        {               
            for (int i = 0; i < boatControls.Length; i++)
            {
                boatControls[i].Move();
               
                yield return new WaitForSeconds(delayTime);
            
         }
            break;
        }
    }

}
