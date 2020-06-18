using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerTransformView : MonoBehaviourPunCallbacks, IPunObservable
{
    public static float smoothCoeff = 2f;

    public static float lerpCoeff = 1f;

    private Vector3 currentPos;
    private Vector3 lastPos;
    private Vector3 vel;
    private float timeBetweenPackets = 0;

    private Rigidbody2D rb;

    private float lerpCounter = 0;

    float prevTime;
    float currentTime;
    void Start()
    {
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 30;
        currentPos = gameObject.transform.position;
        lastPos = gameObject.transform.position;
        vel = Vector2.zero;
        prevTime = Time.realtimeSinceStartup;
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if(!photonView.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, currentPos + lerpCoeff * vel * Time.fixedDeltaTime, Time.fixedDeltaTime * smoothCoeff);
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting && photonView.IsMine)
        {
            currentPos = gameObject.transform.position;
            currentTime = Time.realtimeSinceStartup;
            stream.SendNext(transform.position);
            stream.SendNext(rb.velocity);

            prevTime = currentTime;
            lastPos = currentPos;
        }
        else
        {
            currentPos = (Vector3)stream.ReceiveNext();
            vel = (Vector3)stream.ReceiveNext();
          
        }
    }
}
