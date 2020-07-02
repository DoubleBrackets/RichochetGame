using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AceCharacterToggleAbility : ToggleAbilityClass
{
    public float movementBonus;
    public float reloadBonus;
    public float shootCooldownBonus;

    private float baseReloadSpeed;
    private float baseShootCooldown;

    void Update()
    {
        if(!photonView.IsMine || !NetworkManager.networkManager.gameStarted)
        {
            return;
        }
        UpdateResourceValues();
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if(resourceValue >= toggleThreshholdValue && !isToggleActive)
            {
                isToggleActive = true;
                photonView.RPC("ToggleOn", RpcTarget.AllViaServer,PhotonNetwork.LocalPlayer.NickName);
            }
        }
    }
    [PunRPC]
    protected override void ToggleOn(string name)
    {
        PlayerShootingScript pShootScript = gameObject.GetComponent<PlayerShootingScript>();
        PlayerMovementScript playerMovementScript = gameObject.GetComponent<PlayerMovementScript>();
        baseReloadSpeed = pShootScript.reloadCooldown;
        baseShootCooldown = pShootScript.shootCooldown;
        playerMovementScript.ChangeMovementBonusRPC(movementBonus);
        pShootScript.reloadCooldown = baseReloadSpeed - reloadBonus;
        pShootScript.shootCooldown = baseShootCooldown - shootCooldownBonus;
        PlayerParticleManager.playerParticleManager.PlayParticle(playerParticleKey + name);
    }
    [PunRPC]
    protected override void ToggleOff(string name)
    {
        PlayerShootingScript pShootScript = gameObject.GetComponent<PlayerShootingScript>();
        PlayerMovementScript playerMovementScript = gameObject.GetComponent<PlayerMovementScript>();
        playerMovementScript.ChangeMovementBonusRPC(0);
        pShootScript.reloadCooldown = baseReloadSpeed;
        pShootScript.shootCooldown = baseShootCooldown;
        PlayerParticleManager.playerParticleManager.StopParticle(playerParticleKey + name);
    }
}
