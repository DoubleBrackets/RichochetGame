using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimIndicatorScript : MonoBehaviour
{
    LineRenderer lineRen;

    private float distance = 200f;

    private void Awake()
    {
        lineRen = gameObject.GetComponent<LineRenderer>();
    }

    void Update()
    {
        if(!NetworkManager.networkManager.gameStarted)
        {
            return;
        }
        Vector2 playerPos = PlayerNetworkingScript.LocalPlayerInstance.gameObject.transform.position;
        Vector2 dir = ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - playerPos).normalized;

        lineRen.positionCount = 2;
        lineRen.SetPosition(0, ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition))-dir*10f);
        lineRen.SetPosition(1, playerPos + dir*distance);
    }
}
