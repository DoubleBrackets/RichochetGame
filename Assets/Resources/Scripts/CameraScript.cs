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
            float dist = (targetpos - (Vector2)transform.position).magnitude;
            newPosition = Vector2.Lerp(transform.position, targetpos,(0.45f + dist) * Time.deltaTime);

/*            float yForce = cameraSubject.GetComponent<Rigidbody2D>().velocity.y;
            if (cameraSubject.GetComponent<Rigidbody2D>().velocity.y > 0)
            {
                yForce /= 4f;
            }
            cameraRb.AddForce(new Vector2(cameraSubject.GetComponent<Rigidbody2D>().velocity.x * 0.5f, yForce));*/
        }
        else if(cameraMode == 1)//No smooth camera, just normal cam
        {
            newPosition = cameraSubject.transform.position;
        }
        //if bounds are active, ensures camera doesn't leave bounds
        if(followBounds && cameraMode == 0)
        {
            float aspRatio = (float)Screen.width / Screen.height;
            float screenWidthBound = _camera.orthographicSize * aspRatio;
            float screenHeightBound = _camera.orthographicSize;

            float xPos = newPosition.x;
            float yPos = newPosition.y;
            //Dampens camera movement near boundary
            
            float yVec = newPosition.y - transform.position.y;
            float xVec = newPosition.x - transform.position.x;

            //If closer to lower bound
            if(upperBound - (transform.position.y + screenHeightBound) > transform.position.y - screenHeightBound - lowerBound)
            {
                if(yVec < 0)//If camera is moving downwards
                {
                    float yRatio = (5-(transform.position.y - screenHeightBound - lowerBound))/5;
                    //Dampen
                    yPos = Mathf.Lerp(yPos, transform.position.y, yRatio);
                }
            }
            else//Closer to upper bound
            {
                if (yVec > 0)//If camera is moving upwards
                {
                    float yRatio = (5 - (upperBound - (transform.position.y + screenHeightBound))) / 5;
                    //Dampen
                    yPos = Mathf.Lerp(yPos, transform.position.y, yRatio);
                }
            }

            //If closer to left bound
            if (rightBound - (transform.position.x + screenWidthBound) > transform.position.x - screenWidthBound - leftBound)
            {
                if (xVec < 0)//If camera is moving downwards
                {
                    float xRatio = (5 - (transform.position.x - screenWidthBound - leftBound)) / 5;
                    //Dampen
                    xPos = Mathf.Lerp(xPos, transform.position.x, xRatio);
                }
            }
            else//Closer to right bound
            {
                if (xVec > 0)//If camera is moving right
                {
                    float xRatio = (5 - (rightBound - (transform.position.x + screenWidthBound))) / 5;
                    //Dampen
                    xPos = Mathf.Lerp(xPos, transform.position.x, xRatio);
                }
            }
            //clamping to bounds
            if (xPos + screenWidthBound > rightBound)
                xPos = rightBound - screenWidthBound;
            else if (xPos - screenWidthBound < leftBound)
                xPos = leftBound + screenWidthBound;
            if (yPos + screenHeightBound > upperBound)
                yPos = upperBound - screenHeightBound;
            else if (yPos - screenHeightBound < lowerBound)
                yPos = lowerBound + screenHeightBound;

            if (boundTransitionTimer > 0)//IF transitioning bounds
             {
                xPos = Mathf.Lerp(gameObject.transform.position.x, xPos, 3f * Time.deltaTime);
                //yPos = Mathf.Lerp(gameObject.transform.position.y, yPos, 4f * Time.deltaTime);
                boundTransitionTimer -= Time.deltaTime;
            }

            newPosition = new Vector2(xPos, yPos);
          
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
