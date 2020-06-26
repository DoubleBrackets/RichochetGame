using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticleScript : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public string particleId;

    private bool setParticleId = false;

    public bool usePlayerNickname = true;

    ParticleSystem pSys;

    public GameObject pV;

    void Start()
    {
        pSys = gameObject.GetComponent<ParticleSystem>();
        PlayerParticleManager.playerParticleManager.setParticleActiveEvent += SetActive;
        PlayerParticleManager.playerParticleManager.playParticleEvent += Play;
        PlayerParticleManager.playerParticleManager.stopParticleEvent += Stop;
    }


    void SetActive(string _id, bool val)
    {
        if(_id.CompareTo(particleId) == 0)
        {
            if(val)
            {
                var c = pSys.main;
                c.maxParticles = 1000;
            }
            else
            {
                var c = pSys.main;
                c.maxParticles = 0;
            }
        }
    }

    void Play(string _id)
    {
        if(!setParticleId)
        {
            setParticleId = true;
            if (usePlayerNickname)
            {
                particleId = particleId + gameObject.GetComponentInParent<PlayerNetworkingScript>().nickName;
            }
        }
        if (_id.CompareTo(particleId) == 0)
        {
            pSys.Play();
        }
    }

    void Stop(string _id)
    {
        if (_id.CompareTo(particleId) == 0)
        {
            pSys.Stop();
        }
    }

}
