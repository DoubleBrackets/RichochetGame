using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerMovementScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameObject LocalPlayerInstance;
    //Components
    
    public Rigidbody2D playerRigidBody;

    //Input vars
    private int horizontalInput;
    private int verticalInput;

    //Movement vars
    public float moveSpeed;
    private float movementForce = 200f;

    //States
    private int movementActive = 0;
    private int slowdownActive = 0;


    private void Awake()
    {
        playerRigidBody = gameObject.GetComponent<Rigidbody2D>();
        if (photonView.IsMine && PhotonNetwork.IsConnected)
        {
            LocalPlayerInstance = gameObject;
            //CameraScript.cameraScript.cameraSubject = gameObject;
        }
    }

    public void Update()
    {
        if(!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }
        horizontalInput = (int)Input.GetAxisRaw("Horizontal");
        verticalInput = (int)Input.GetAxisRaw("Vertical");
    }

    public void FixedUpdate()
    {
        Vector2 inputVector = GetInputVector();
        //Horizontal movement
        if(movementActive == 0)
        {
            float yVelAfterForce = playerRigidBody.velocity.y + (movementForce * inputVector.y / playerRigidBody.mass) * Time.fixedDeltaTime;
            float xVelAfterForce = playerRigidBody.velocity.x + (movementForce * inputVector.x / playerRigidBody.mass) * Time.fixedDeltaTime;

            Vector2 velAfterForce = new Vector2(xVelAfterForce, yVelAfterForce);
            if(velAfterForce.magnitude <= moveSpeed)//If force does not go over movespeed, apply force 
            {
                playerRigidBody.velocity = velAfterForce;
            }
            else//Otherwise directly set velocity
            {
                if (verticalInput != 0)
                    SetYVelocity(inputVector.y * moveSpeed);
                if (horizontalInput != 0)
                    SetXVelocity(inputVector.x * moveSpeed);
            }
        }
        //Slowdown
        bool _slowDownActive = (movementActive > 0);
        if(slowdownActive == 0)
        {
            if (horizontalInput == 0 || _slowDownActive)
                SetXVelocity(playerRigidBody.velocity.x * 0.8f);
            if (verticalInput == 0 || _slowDownActive)
                SetYVelocity(playerRigidBody.velocity.y * 0.8f);
        }

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && photonView.IsMine)
        {
            stream.SendNext(horizontalInput);
            stream.SendNext(verticalInput);
        }
        else
        {
            horizontalInput =(int) stream.ReceiveNext();
            verticalInput = (int)stream.ReceiveNext();
        }
    }


    public Vector2 GetInputVector()
    {
        return new Vector2(horizontalInput, verticalInput).normalized;
    }

    public Vector2 GetMouseVector()
    {
        Vector2 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - gameObject.transform.position;
        return diff;
    }

    public void SetXVelocity(float xVel)
    {
        playerRigidBody.velocity = new Vector2(xVel, playerRigidBody.velocity.y);
    }
    public void SetYVelocity(float yVel)
    {
        playerRigidBody.velocity = new Vector2(playerRigidBody.velocity.x,yVel);
    }

    public void IncrementMovementActive()
    {
        movementActive++;
    }

    public void DecrementMovementActive()
    {
        movementActive--;
    }

    public void IncrementSlowdownActive()
    {
        slowdownActive++;
    }

    public void DecrementSlowdownActive()
    {
        slowdownActive--;
    }
}
