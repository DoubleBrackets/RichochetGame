using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    //Singleton
    public static CameraScript cameraScript;


    public GameObject cameraSubject;
    private Camera _camera;
    public int cameraMode = 0;//0 is follow player
    private Rigidbody2D cameraRb;
    //bounds
    private bool followBounds = false;
    private float lowerBound, upperBound, leftBound, rightBound;

    Vector2  newPosition;
    //Bounds transitions
    private float boundTransitionTime = 2.5f;
    private float boundTransitionTimer = 0;
    void Awake()
    {
        cameraScript = this;
        _camera = gameObject.GetComponent<Camera>();
        cameraRb = gameObject.GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (cameraSubject == null)
            return;
        newPosition = transform.position;
        //Follows player
        

        if (cameraMode == 0)
        {
            
            Vector2 targetpos = cameraSubject.transform.position;
            //if bounds are active, clamp targetPos to the bounds
            if (followBounds)
            {
                float aspRatio = (float)Screen.width / Screen.height;
                float screenWidthBound = _camera.orthographicSize * aspRatio;
                float screenHeightBound = _camera.orthographicSize;

                float boundXPos = Mathf.Clamp(targetpos.x, leftBound + screenWidthBound, rightBound - screenWidthBound);
                float boundYPos = Mathf.Clamp(targetpos.y, lowerBound + screenHeightBound, upperBound - screenHeightBound);

                targetpos = new Vector2(boundXPos, boundYPos);
            }
            float dist = (targetpos - (Vector2)transform.position).magnitude;
            newPosition = Vector2.Lerp(transform.position, targetpos,(0.45f + dist) * Time.deltaTime);
        }
        else if(cameraMode == 1)//No smooth camera, just normal cam
        {
            newPosition = cameraSubject.transform.position;
        }
        transform.position = newPosition;
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -10f);

    }

    public void SetBounds(float left, float right, float upper, float lower)
    {
        boundTransitionTimer = boundTransitionTime;
        lowerBound = lower;
        upperBound = upper;
        leftBound = left;
        rightBound = right;
    }

    public void SetLeftBound(float left)
    {
        boundTransitionTimer = boundTransitionTime;
        leftBound = left;
    }
    public void SetRightBound(float right)
    {
        boundTransitionTimer = boundTransitionTime;
        rightBound = right;
    }
    public void SetUpperBound(float upper)
    {
        boundTransitionTimer = boundTransitionTime;
        upperBound = upper;
    }
    public void SetLowerBound(float lower)
    {
        boundTransitionTimer = boundTransitionTime;
        lowerBound = lower;
    }
    public void SetFollowBounds(bool val)
    {
        followBounds = val;
    }

    public void CameraShake(float magnitude)
    {
        Vector2 dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * magnitude;
        cameraRb.AddForce(dir / Time.fixedDeltaTime);
    }


    public void CameraShake(Vector2 dir, float magnitude)
    {
        cameraRb.AddForce(dir.normalized * magnitude/Time.fixedDeltaTime);
    }
}
