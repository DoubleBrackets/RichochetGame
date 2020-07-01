using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerMovementScript : MonoBehaviourPunCallbacks, IPunObservable
{
    //Components

    public Rigidbody2D playerRigidBody;

    //Input vars
    private int horizontalInput;
    private int verticalInput;

    //Movement vars
    public float moveSpeed;
    private float movementForce = 200f;

    private float movementBonus = 0f;

    //States
    private int movementActive = 0;
    private int slowdownActive = 0;

    //Dashing
    private bool isDashing = false;
    private Vector2 dashVector = Vector2.zero;
    private float dashMagnitude = 40f;

    private float dashingCooldown = 1f;
    private float dashingCooldownTimer = 0;




    private void Awake()
    {
        playerRigidBody = gameObject.GetComponent<Rigidbody2D>();
        if (photonView.IsMine && PhotonNetwork.IsConnected)
        {
            CameraScript.cameraScript.cameraSubject = gameObject;
        }
    }

    public void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }
        horizontalInput = (int)Input.GetAxisRaw("Horizontal");
        verticalInput = (int)Input.GetAxisRaw("Vertical");
        if (!NetworkManager.networkManager.gameStarted)
            return;
        if (dashingCooldownTimer > 0)
        {
            dashingCooldownTimer -= Time.deltaTime;
        }

        if (GetInputVector() != Vector2.zero)
            dashVector = GetInputVector();
        if (photonView.IsMine && dashingCooldownTimer <= 0)
        {
            //Gets dash vector
            if ((Input.GetMouseButtonDown(1)))
            {
                dashVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                dashVector = dashVector / (dashVector.magnitude);
            }
            else if (!(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space)))
            {
                return;
            }
            dashingCooldownTimer = dashingCooldown;
            photonView.RPC("PlayerDash", RpcTarget.All, (Vector3)dashVector);
        }
    }

    [PunRPC]
    private void PlayerDash(Vector3 _dashVector)
    {
        isDashing = true;
        dashVector = _dashVector;
        StartCoroutine(PlayerDashMain());
    }
    IEnumerator PlayerDashMain()
    {
        slowdownActive++;
        movementActive++;

        playerRigidBody.velocity = dashVector * dashMagnitude;

        if (photonView.IsMine)
            gameObject.layer = 11;

        yield return new WaitForSeconds(0.3f);
        slowdownActive--;
        movementActive--;

        yield return new WaitForSeconds(0.1f);
        playerRigidBody.velocity /= 5f;
        if (photonView.IsMine)
            gameObject.layer = 9;
        yield return new WaitForSeconds(0.05f);

        isDashing = false;
    }

    public void FixedUpdate()
    {
        if (!NetworkManager.networkManager.gameStarted)
        {
            return;
        }
        Vector2 inputVector = GetInputVector();
        //Horizontal movement
        if (movementActive == 0)
        {
            float yVelAfterForce = playerRigidBody.velocity.y + (movementForce * inputVector.y / playerRigidBody.mass) * Time.fixedDeltaTime;
            float xVelAfterForce = playerRigidBody.velocity.x + (movementForce * inputVector.x / playerRigidBody.mass) * Time.fixedDeltaTime;

            Vector2 velAfterForce = new Vector2(xVelAfterForce, yVelAfterForce);
            if (velAfterForce.magnitude <= moveSpeed + movementBonus)//If force does not go over movespeed, apply force 
            {
                playerRigidBody.velocity = velAfterForce;
            }
            else//Otherwise directly set velocity
            {
                if (verticalInput != 0)
                    SetYVelocity(inputVector.y * (moveSpeed + movementBonus));
                if (horizontalInput != 0)
                    SetXVelocity(inputVector.x * (moveSpeed + movementBonus));
            }
        }
        //Slowdown
        bool _slowDownActive = (movementActive > 0);
        if (slowdownActive == 0)
        {
            if (horizontalInput == 0 || _slowDownActive)
                SetXVelocity(playerRigidBody.velocity.x * 0.75f);
            if (verticalInput == 0 || _slowDownActive)
                SetYVelocity(playerRigidBody.velocity.y * 0.75f);
        }

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && photonView.IsMine)
        {
            stream.SendNext(horizontalInput);
            stream.SendNext(verticalInput);
            //stream.SendNext(isDashing);
            //stream.SendNext((Vector3)dashVector);
        }
        else
        {
            horizontalInput = (int)stream.ReceiveNext();
            verticalInput = (int)stream.ReceiveNext();
            //isDashing = (bool)stream.ReceiveNext();
            //dashVector = (Vector3)stream.ReceiveNext();
        }
    }

    public void ChangeMovementBonusRPC(float val)
    {
        photonView.RPC("ChangeMovementBonus", RpcTarget.All, val);
    }

    [PunRPC]

    private void ChangeMovementBonus(float val)
    {
        movementBonus = val;
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

    public int GetHorizontalInput()
    {
        return horizontalInput;
    }

    public int GetVerticalInput()
    {
        return verticalInput;
    }

    public bool GetIsDashing()
    {
        return isDashing;
    }

    public float GetDashingHorizontal()
    {
        return dashVector.x;
    }

    public float GetDashingVertical()
    {
        return dashVector.y;
    }
}
