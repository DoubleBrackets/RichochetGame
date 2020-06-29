using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerNetworkingScript : MonoBehaviourPunCallbacks
{
    //Deals with player features that require networking(mainly UI), excluding movement and controls

    public static GameObject LocalPlayerInstance;

    public Text nameTag;
    public Text ammoTag;
    public Image reloadBar;

    public string nickName;

    //Tracking arrow 
    public GameObject trackingArrow;
    private float arrowOffset = 15f;

    public Image positionArrow;

    private void Awake()//Sets nametag on player instantiate
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SetNameTag", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName);
            positionArrow.enabled = true;
        }
        else
            positionArrow.enabled = false;
    }

    private void Update()
    {
        /*
        if (!NetworkManager.networkManager.gameStarted || NetworkManager.networkManager.opponentGameObject == null || !photonView.IsMine)
            return;
        Vector2 arrowDir = NetworkManager.networkManager.opponentGameObject.transform.position - gameObject.transform.position;
        if(arrowDir.magnitude <= 50f)
        {
            trackingArrow.SetActive(false);
        }
        else
        {
            trackingArrow.SetActive(true);
        }
        float angle = Mathf.Rad2Deg*Mathf.Atan2(arrowDir.y, arrowDir.x);
        trackingArrow.transform.position = (Vector2)transform.position + (arrowDir.normalized)*arrowOffset;
        trackingArrow.transform.rotation = Quaternion.Euler(0, 0, angle);
        */
    }

    //Player dies when hit by projectile
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.gameObject.layer == 10)//Is a projectile
        {
            StartCoroutine(PlayerHit());           
        }
    }

    IEnumerator PlayerHit()
    {
        photonView.RPC("PlayDeathEffects", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
        NetworkManager.networkManager.PlayerDeathRPC(PhotonNetwork.LocalPlayer.NickName);
        yield return new WaitForSeconds(1.5f);
        ScreenUIScript.screenUIScript.ShowDeathMessageRPC(PhotonNetwork.LocalPlayer.NickName,true);
    }

    [PunRPC]

    private void PlayDeathEffects(string str)
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        PlayerParticleManager.playerParticleManager.PlayParticle("DeathParticle"+ str);
    }




    [PunRPC]

    private void SetNameTag(string val)
    {
        nameTag.text = val;
        nickName = val;
    }
    
    public void SpawnPlayerRPC(Vector3 pos)
    {
        photonView.RPC("SpawnPlayer", RpcTarget.AllBuffered, pos);
    }

    [PunRPC]

    private void SpawnPlayer(Vector3 pos)
    {
        transform.position = pos;
    }

    public void SetAmmoValueRPC(int val,int max)
    {
        photonView.RPC("SetAmmoValue", RpcTarget.AllBuffered, val,max);
    }

    [PunRPC]

    private void SetAmmoValue(int val,int max)
    {
        ammoTag.text = "" + val +"/" + max;
    }

    public void UpdateReloadBarRPC(float val, float max)
    {
        photonView.RPC("UpdateReloadBar", RpcTarget.AllBuffered, val, max);
    }

    [PunRPC]

    private void UpdateReloadBar(float val, float max)
    {
        reloadBar.transform.localScale = new Vector2(val / max,1);
    }





}
