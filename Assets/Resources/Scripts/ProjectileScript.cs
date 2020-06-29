using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.Asteroids;

public class ProjectileScript : MonoBehaviourPunCallbacks
{
    public float speed;
    public float speedAfterBounce;
    public Rigidbody2D rigidBody;
    public LineRenderer lineRen;
    public ParticleSystem onCollisionParticles;
    public ParticleSystem onBreakParticles;
    private ParticleSystem bulletParticle;

    private Vector2 savedVelocityOnBounce;

    public float bounceDelay;

    public float screenShakeMagnitude;

    private float bounceRandomness = 5f;//Degrees of randomness when bouncing

    private bool isInBounce = false;

    public LayerMask indicatorRaycastMask;
    private float widthMult;

    private float startTime;

    private float radius;

    private Vector3 bounceLocation;

    public int numberOfBounces = 2;
    private int bounceCounter;

    private void Awake()
    {
        bulletParticle = gameObject.GetComponent<ParticleSystem>();
        widthMult = lineRen.widthMultiplier;
        radius = gameObject.GetComponent<Collider2D>().bounds.extents.x;
        onCollisionParticles.Stop();
        onBreakParticles.Stop();
        bounceCounter = numberOfBounces+1;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        speed = speedAfterBounce;
        Vector2 currentVel = rigidBody.velocity;
        onCollisionParticles.Play();
        transform.position = bounceLocation;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(currentVel.y,currentVel.x)*Mathf.Rad2Deg);
        if (!PhotonNetwork.IsMasterClient || !NetworkManager.networkManager.gameStarted)//if not master client, then wait for bounce position/vel from master client
        {
            rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
            return;
        }
        photonView.RPC("OnBulletBounce",RpcTarget.All,transform.position,(Vector3)currentVel, bounceDelay,(float)PhotonNetwork.Time);
    }
    [PunRPC]
    public void OnBulletBounce(Vector3 position, Vector3 vel,float delay,float sendTime)
    {
        bounceCounter--;
        StartCoroutine(OnBulletBounceMain(position, vel, delay,sendTime));
    }


    private IEnumerator OnBulletBounceMain(Vector3 position, Vector3 vel, float delay,float rpcSendTime)
    {
        isInBounce = true;

        transform.position = position;
        savedVelocityOnBounce = vel;
        float angle = Mathf.Rad2Deg * Mathf.Atan2(savedVelocityOnBounce.y, savedVelocityOnBounce.x)+0.5f;
        savedVelocityOnBounce = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * speed;

        rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
        //Creates line indicator
        
        lineRen.SetPosition(0, transform.position);
        RaycastHit2D rayCast = Physics2D.CircleCast(transform.position, radius,savedVelocityOnBounce, 1000f,indicatorRaycastMask);
        if(rayCast.collider != null && bounceCounter != 0)
        {
            lineRen.widthMultiplier = widthMult;
            lineRen.enabled = true;
            lineRen.SetPosition(1, rayCast.centroid);
            bounceLocation = rayCast.centroid-savedVelocityOnBounce.normalized*radius;
        }
        float reduction = 0;
        if (!PhotonNetwork.IsMasterClient)
            reduction = (float)PhotonNetwork.Time - rpcSendTime;
        yield return new WaitForSeconds(Mathf.Max(0,delay- reduction));
        if (bounceCounter == 0)
        {
            bulletParticle.Stop();
            onBreakParticles.Play();
            gameObject.GetComponent<Collider2D>().enabled = false;
            Destroy(this.gameObject,0.5f);
            yield break;
        }
        rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

        rigidBody.velocity = savedVelocityOnBounce;

        isInBounce = false;
        //Line fadeout animation
        for(int x = 0;x <= 20;x++)
        { 
            lineRen.widthMultiplier = widthMult*(1 - x / 20f);
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
