using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShotgunCharacterToggleAbility : ToggleAbilityClass, IPunObservable
{
    private float baseReloadSpeed;
    private float baseShootCooldown;

    private float movementForce = 1250f;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!photonView.IsMine || !NetworkManager.networkManager.gameStarted)
        {
            return;
        }
        UpdateResourceValues();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (resourceValue >= toggleThreshholdValue && !isToggleActive)
            {
                isToggleActive = true;
                photonView.RPC("ToggleOn", RpcTarget.AllViaServer);
            }
        }

        if(isToggleActive)
        {
            Vector2 dir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            rb.AddForce(dir.normalized * movementForce* Time.deltaTime);
        }
    }


    [PunRPC]
    protected override void ToggleOn()
    {
        PlayerShootingScript pShootScript = gameObject.GetComponent<PlayerShootingScript>();
        PlayerMovementScript playerMovementScript = gameObject.GetComponent<PlayerMovementScript>();
        gameObject.layer = 11;
        pShootScript.IncrementCanShoot();
        playerMovementScript.IncrementSlowdownActive();
        playerMovementScript.IncrementMovementActive();
        playerMovementScript.IncrementCanDash();
        PlayerParticleManager.playerParticleManager.PlayParticle(playerParticleKey + PhotonNetwork.LocalPlayer.NickName);
        
    }
    [PunRPC]
    protected override void ToggleOff()
    {
        PlayerShootingScript pShootScript = gameObject.GetComponent<PlayerShootingScript>();
        PlayerMovementScript playerMovementScript = gameObject.GetComponent<PlayerMovementScript>();
        if (NetworkManager.networkManager.gameStarted)
            gameObject.layer = 9;
        pShootScript.DecrementCanShoot();
        playerMovementScript.DecrementSlowdownActive();
        playerMovementScript.DecrementMovementActive();
        playerMovementScript.DecrementCanDash();
        PlayerParticleManager.playerParticleManager.StopParticle(playerParticleKey + PhotonNetwork.LocalPlayer.NickName);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!isToggleActive)
            return;
        if (stream.IsWriting && photonView.IsMine)
        {
            stream.SendNext((Vector3)rb.velocity);
        }
        else
        {
            rb.velocity = (Vector3)stream.ReceiveNext();          
        }
    }
}
