using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimIndicatorScript : MonoBehaviour
{
    public static AimIndicatorScript aimIndicatorScript;
    LineRenderer lineRen;

    private float distance = 150f;

    private LayerMask raycastMask;

    private void Awake()
    {
        raycastMask = LayerMask.GetMask(new string[] { "Terrain" });
        lineRen = gameObject.GetComponent<LineRenderer>();
        aimIndicatorScript = this;
    }

    void Update()
    {
        if(!NetworkManager.networkManager.gameStarted)
        {
            return;
        }
        Vector2 mousePos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerPos = PlayerNetworkingScript.LocalPlayerInstance.gameObject.transform.position;
        Vector2 dir = ( mousePos - playerPos).normalized;

        lineRen.positionCount = 2;
        lineRen.SetPosition(0, playerPos);
        RaycastHit2D rc = Physics2D.Raycast(playerPos, dir, distance, raycastMask);
        if(rc.collider != null)
        {
            lineRen.SetPosition(1, rc.point);
        }
        else
            lineRen.SetPosition(1, playerPos + dir * distance);
    }
}
