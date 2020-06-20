using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerTransformView : MonoBehaviourPunCallbacks, IPunObservable
{
    public static float smoothCoeff = 5f;

    public static float lerpCoeff = 0.5f;

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
        if(!photonView.IsMine)
        {
            gameObject.layer = 11;
        }
        
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 20;
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
            
            stream.SendNext(transform.position);
            stream.SendNext((Vector3)rb.velocity);

           
            lastPos = currentPos;
        }
        else
        {
            currentPos = (Vector3)stream.ReceiveNext();
            vel = (Vector3)stream.ReceiveNext();
            currentTime = Time.realtimeSinceStartup;
            prevTime = currentTime;
        }
    }
}
