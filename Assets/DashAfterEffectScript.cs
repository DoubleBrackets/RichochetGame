using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashAfterEffectScript : MonoBehaviour
{
    // Start is called before the first frame update
    public SpriteRenderer spriteRen;
    void Awake()
    {
        StartCoroutine(AfterEffect());
    }
    
    IEnumerator AfterEffect()
    {
        Color c;
        for(int x = 0;x <= 20;x++)
        {
            c = spriteRen.color;
            c.a = (1 - x / 20f)*0.5f;
            spriteRen.color = c;
            yield return new WaitForFixedUpdate();
        }

        Destroy(gameObject);
    }

}
