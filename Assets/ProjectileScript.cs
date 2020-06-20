using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ProjectileScript : MonoBehaviourPunCallbacks
{
    public float speed;
    public Rigidbody2D rigidBody;
    public LineRenderer lineRen;
    public ParticleSystem onCollisionParticles;

    private Vector2 savedVelocityOnBounce;

    private float bounceDelay = 0.25f;

    private float bounceRandomness = 5f;//Degrees of randomness when bouncing

    private bool isInBounce = false;

    public LayerMask indicatorRaycastMask;
    private float widthMult;

    private float startTime;

    private void Awake()
    {
        widthMult = lineRen.widthMultiplier;
        onCollisionParticles.Stop();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        onCollisionParticles.Play();
        if(!PhotonNetwork.IsMasterClient)//if not master client, then wait for bounce position/vel from master client
        {
            transform.rotation = Quaternion.Euler(0, 0, Vector2.Angle(Vector2.zero,rigidBody.velocity) + 90f);
            rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
            return;
        }
        photonView.RPC("OnBulletBounce",RpcTarget.All,transform.position,(Vector3)rigidBody.velocity,bounceDelay,(float)PhotonNetwork.Time);
    }
    [PunRPC]
    public void OnBulletBounce(Vector3 position, Vector3 vel,float delay,float sendTime)
    {
        StartCoroutine(OnBulletBounceMain(position, vel, delay,sendTime));
    }


    private IEnumerator OnBulletBounceMain(Vector3 position, Vector3 vel, float delay,float rpcSendTime)
    {
        isInBounce = true;

        transform.position = position;
        savedVelocityOnBounce = vel;
        float angle = Mathf.Rad2Deg * Mathf.Atan2(savedVelocityOnBounce.y, savedVelocityOnBounce.x) + 1;
        savedVelocityOnBounce = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * speed;

        //Rotates to face direction of travel
        transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
        rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
        //Creates line indicator
        
        lineRen.SetPosition(0, transform.position);
        RaycastHit2D rayCast = Physics2D.Raycast(transform.position, savedVelocityOnBounce, 1000f,indicatorRaycastMask);
        if(rayCast.collider != null)
        {
            lineRen.widthMultiplier = widthMult;
            lineRen.enabled = true;
            lineRen.SetPosition(1, rayCast.point);
        }
        float reduction = 0;
        if (!PhotonNetwork.IsMasterClient)
            reduction = (float)PhotonNetwork.Time - rpcSendTime;
        yield return new WaitForSeconds(Mathf.Max(0,delay- reduction));
        rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

        rigidBody.velocity = savedVelocityOnBounce;

        isInBounce = false;
        //Line fadeout animation
        for(int x = 0;x <= 10;x++)
        { 
            lineRen.widthMultiplier = widthMult*(1 - x / 10f);
            yield return new WaitForFixedUpdate();
            if (isInBounce == true)
            {
                yield break;
            }
        }
        lineRen.widthMultiplier = widthMult;
        lineRen.enabled = false;
    }
}
