﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevePointScript : MonoBehaviour
{
    // Start is called before the first frame update
    //Bounds
    public GameObject leftBound;
    public GameObject rightBound;
    public GameObject upperBound;
    public GameObject lowerBound;

    public bool FollowBounds = true;
    //RespawnPoint
    public GameObject respawnPoint;

    public bool setNewRespawn = true;
    // Update is called once per frame

    private void Start()
    {
        //Sets bounds
        CameraScript camScript = CameraScript.cameraScript;
        if(FollowBounds)
        {
            camScript.SetFollowBounds(true);
            if (leftBound != null)
                camScript.SetLeftBound(leftBound.transform.position.x);
            if (rightBound != null)
                camScript.SetRightBound(rightBound.transform.position.x);
            if (upperBound != null)
                camScript.SetUpperBound(upperBound.transform.position.y);
            if (lowerBound != null)
                camScript.SetLowerBound(lowerBound.transform.position.y);
        }

        //if (setNewRespawn)
            // PlayerManager.playerManager.SetRespawnPosition(respawnPoint.transform.position); 
    }

}
