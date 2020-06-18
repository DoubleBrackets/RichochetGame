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

    private Vector2 savedVelocityOnBounce;

    private float bounceDelay = 0.2f;

    private float bounceRandomness = 5f;//Degrees of randomness when bouncing
    private float radius;

    private bool isInBounce = false;

    public LayerMask indicatorRaycastMask;
    private float widthMult;

    private void Awake()
    {
        radius = gameObject.GetComponent<CircleCollider2D>().bounds.extents.x;
        widthMult = lineRen.widthMultiplier;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
            return;
        }
        StartCoroutine(OnCollision());
    }

    IEnumerator OnCollision()
    {
        yield return new WaitForFixedUpdate();
        float ran = Random.Range(-bounceRandomness, bounceRandomness);
        Vector2 vel = rigidBody.velocity;
        rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
        photonView.RPC("OnBulletBounce", RpcTarget.AllViaServer, transform.position, ran, bounceDelay, (Vector3)vel);

    }

    public void OnBulletShoot(Vector3 pos,float random, float delay,Vector3 vel)
    {
        StartCoroutine(OnBulletBounceMain(pos,random, delay,vel));
    }

    [PunRPC]
    public void OnBulletBounce(Vector3 pos, float random, float delay, Vector3 vel)
    {
        StartCoroutine(OnBulletBounceMain(pos, random, delay, vel));
    }

    private IEnumerator OnBulletBounceMain(Vector3 pos,float random,float delay,Vector3 vel)
    {
        isInBounce = true;
        transform.position = pos;

        savedVelocityOnBounce = vel;
        float angle = Mathf.Rad2Deg * Mathf.Atan2(savedVelocityOnBounce.y, savedVelocityOnBounce.x) + random;
        savedVelocityOnBounce = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * speed;

        //Rotates to face direction of travel
        rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90f);

        //Creates line indicator

        lineRen.SetPosition(0, transform.position);
        RaycastHit2D rayCast = Physics2D.CircleCast(transform.position, radius, savedVelocityOnBounce, 100f,indicatorRaycastMask);
        if(rayCast.collider != null)
        {
            lineRen.widthMultiplier = widthMult;
            lineRen.enabled = true;
            lineRen.SetPosition(1, rayCast.point);
        }
        
        yield return new WaitForSeconds(delay);
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
