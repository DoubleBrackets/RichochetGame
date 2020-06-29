using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScript : MonoBehaviour
{
    public GameObject p1Spawn;
    public GameObject p2Spawn;

    public GameObject leftBound, rightBound, upperBound, lowerBound;

    public string mapName;

    public GameObject mapTarget;

    private void Start()
    {
        //Calculating camera size
        float aspRatio = (float)Screen.width / Screen.height;
        float minCameraWidth = rightBound.transform.position.x - leftBound.transform.position.x;
        float minCameraHeight = upperBound.transform.position.y - lowerBound.transform.position.y;

        if(minCameraHeight * aspRatio >= minCameraWidth)//Camera needs to fit height
        {
            Camera.main.orthographicSize = minCameraHeight / 2;
        }
        else//Needs to fit width
        {
            Camera.main.orthographicSize = minCameraWidth / (aspRatio * 2);
        }

        CameraScript.cameraScript.cameraSubject = mapTarget;
    }
}
