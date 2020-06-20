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
    public Image reloadBar;

    public string nickName;

    private void Awake()//Sets nametag on player instantiate
    {
        if(photonView.IsMine)
            photonView.RPC("SetNameTagMain", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName);
    }

    //Player dies when hit by projectile
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.gameObject.layer == 10)//Is a projectile
        {
            gameObject.layer = 11;
            gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            StartCoroutine(PlayerHit());           
        }
    }

    IEnumerator PlayerHit()
    {
        photonView.RPC("PlayDeathEffects", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
        yield return new WaitForSeconds(1f);
        NetworkManager.networkManager.PlayerDeath(PhotonNetwork.LocalPlayer.NickName);
    }

    [PunRPC]

    private void PlayDeathEffects(string str)
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        PlayerParticleManager.playerParticleManager.PlayParticle("DeathParticle"+ str);
    }

    [PunRPC]

    private void SetNameTagMain(string val)
    {
        nameTag.text = val;
        nickName = val;
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

    public void UpdateReloadBar(float val, float max)
    {
        photonView.RPC("UpdateReloadBarMain", RpcTarget.AllBuffered, val, max);
    }

    [PunRPC]

    private void UpdateReloadBarMain(float val, float max)
    {
        reloadBar.transform.localScale = new Vector2(val / max,1);
    }


}
