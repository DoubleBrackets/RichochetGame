using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticleManager : MonoBehaviourPunCallbacks
{
    public static PlayerParticleManager playerParticleManager;

    private void Awake()
    {
        if(photonView.IsMine)
            playerParticleManager = this;
    }


    //Event system for player particles
    public event Action<String> playParticleEvent;
    public void PlayParticle(string _id)
    {
        playParticleEvent?.Invoke(_id);
    }

    public void PlayParticleRPC(string _id)
    {
        photonView.RPC("PlayParticleRPCMain", RpcTarget.All, _id);
    }

    [PunRPC]

    private void PlayParticleRPCMain(string _id)
    {
        playParticleEvent?.Invoke(_id);
    }

    public event Action<String> stopParticleEvent;
    public void StopParticle(string _id)
    {
        stopParticleEvent?.Invoke(_id);
    }

    public event Action<String,bool> setParticleActiveEvent;
    public void SetParticleActive(string _id, bool val)
    {
        if(setParticleActiveEvent != null)
        {
            setParticleActiveEvent(_id, val);
        }
    }

}
