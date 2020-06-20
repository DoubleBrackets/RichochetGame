using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerNetworkingScript : MonoBehaviourPunCallbacks
{

    public Text nameTag;
    public Text ammoTag;

    private void Awake()//Sets nametag on player instantiate
    {
        if(photonView.IsMine)
            photonView.RPC("SetNameTagMain", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName);
    }


    [PunRPC]

    private void SetNameTagMain(string val)
    {
        nameTag.text = val;
    }
    
    public void SpawnPlayer(Vector3 pos)
    {
        photonView.RPC("SpawnPlayerMain", RpcTarget.AllBuffered, pos);
    }

    [PunRPC]

    private void SpawnPlayerMain(Vector3 pos)
    {
        transform.position = pos;
    }

    public void SetAmmoValue(int val,int max)
    {
        photonView.RPC("SetAmmoValueMain", RpcTarget.AllBuffered, val,max);
    }

    [PunRPC]

    private void SetAmmoValueMain(int val,int max)
    {
        ammoTag.text = "" + val +"/" + max;
    }


}
