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
    public bool slowdownAfterShot = false;
    public float slowdownInterval = 0f;
    public bool slowdownAfterBounce = true;

    public Rigidbody2D rigidBody;
    public LineRenderer lineRen;
    public ParticleSystem onCollisionParticles;
    public ParticleSystem onBreakParticles;
    private ParticleSystem bulletParticle;
    private Collider2D coll;

    private Vector2 savedVelocityOnBounce;

    public float bounceDelay;

    public float screenShakeMagnitude;

    private float bounceRandomness = 5f;//Degrees of randomness when bouncing

    private bool isInBounce = false;

    private LayerMask indicatorRaycastMask;
    private float widthMult;

    private float startTime;

    private float radius;

    private Vector2 projectileBounceVector;

    public int numberOfBounces = 2;
    private int bounceCounter;

    private void Awake()
    {
        bulletParticle = gameObject.GetComponent<ParticleSystem>();
        widthMult = lineRen.widthMultiplier;
        coll = gameObject.GetComponent<Collider2D>();
        radius = coll.bounds.extents.x;
        onCollisionParticles.Stop();
        onBreakParticles.Stop();
        bounceCounter = numberOfBounces+1;
        indicatorRaycastMask = LayerMask.GetMask(new string[]{"Terrain" });
    }

    private void OnCollisionReached()
    {

        if(slowdownAfterBounce)
            speed = speedAfterBounce;
        onCollisionParticles.Play();
        Vector2 currentVel = projectileBounceVector;
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
        if (slowdownAfterShot)
        {
            slowdownAfterShot = false;
            StartCoroutine(ShotSlowdown());
        }
        bounceCounter--;
        StartCoroutine(OnBulletBounceMain(position, vel, delay,sendTime));
    }


    private IEnumerator OnBulletBounceMain(Vector3 position, Vector3 vel, float delay,float rpcSendTime)
    {
        isInBounce = true;

        transform.position = position;
        savedVelocityOnBounce = vel;

        rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
        //Calculates bounce

        lineRen.SetPosition(0, transform.position);
        RaycastHit2D rayCast = Physics2D.Raycast(transform.position, vel, 1000f, indicatorRaycastMask);
        if (rayCast.collider != null && bounceCounter != 0)
        {
            lineRen.widthMultiplier = widthMult;
            lineRen.enabled = true;
            lineRen.SetPosition(1, rayCast.point - (Vector2)vel.normalized*radius);
            projectileBounceVector = Vector2.Reflect(vel, rayCast.normal);//Saves the vector for when it bounces next
        }
        float reduction = 0;
        if (!PhotonNetwork.IsMasterClient)
            reduction = (float)PhotonNetwork.Time - rpcSendTime;
        yield return new WaitForSeconds(Mathf.Max(0,delay- reduction));

        //Destroy projectile if bounce limit reached
        if (bounceCounter == 0)
        {
            bulletParticle.Stop();
            onBreakParticles.Play();
            gameObject.GetComponent<Collider2D>().enabled = false;
            Destroy(this.gameObject,0.5f);
            yield break;
        }
        rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

        StartCoroutine(StartProjectilePath(savedVelocityOnBounce, rayCast.point - (Vector2)vel.normalized * radius));

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

    IEnumerator StartProjectilePath(Vector2 vel,Vector2 target)//Uses raycasts as a guide for the projectiles path, times when projectile would normally collide with terrain
    {
        int bounceCounterLocal = bounceCounter;
        rigidBody.velocity = savedVelocityOnBounce.normalized * speed;
        Vector2 diff = target - (Vector2)transform.position;
        if (slowdownInterval == 0)//If there is no slowdown immediately following a shot
        {            
            float time = diff.magnitude / speed;
            yield return new WaitForSeconds(time);
            if (bounceCounterLocal != bounceCounter)//Master client projectiles take priority
                yield break;
            transform.position = target;
            OnCollisionReached();
        }
        else//if slowing down, then watches projectile position rather than calculating the time
        {
            Vector2 startPos = transform.position;
            float diffMag = diff.magnitude;
            while(true)
            {
                if (((Vector2)transform.position - startPos).magnitude >= diffMag)
                    break;
                yield return new WaitForFixedUpdate();
            }
            if (bounceCounterLocal != bounceCounter)//Master client projectiles take priority
                yield break;
            transform.position = target;
            OnCollisionReached();
        }
    }

    IEnumerator ShotSlowdown()
    {
        float initialSpeed = speed;
        float currentTime = Time.time;
        float startTime = Time.time;
        do
        {
            currentTime = Time.time;
            speed = initialSpeed - ((currentTime - startTime) / slowdownInterval) * (initialSpeed - speedAfterBounce);
            rigidBody.velocity = rigidBody.velocity.normalized * speed;
            yield return new WaitForFixedUpdate();
        } while (currentTime - startTime <= slowdownInterval);
        speed = speedAfterBounce;
        rigidBody.velocity = rigidBody.velocity.normalized * speed;
        slowdownInterval = 0;
    }
}
