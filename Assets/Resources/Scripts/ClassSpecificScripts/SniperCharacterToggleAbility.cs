﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SniperCharacterToggleAbility : ToggleAbilityClass
{
    public float movementBonus;
    public float reloadBonus;
    public float shootCooldownBonus;

    public int numberOfBouncesShown;

    private float baseReloadSpeed;
    private float baseShootCooldown;

    private LineRenderer aimLineRen;

    private LayerMask indicatorRaycastMask;

    private void Awake()
    {
        aimLineRen = gameObject.GetComponentInChildren<LineRenderer>();
        aimLineRen.positionCount = numberOfBouncesShown + 2;
        indicatorRaycastMask = LayerMask.GetMask(new string[] { "Terrain" });
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
            CreateAimLines();
        }
    }

    private void CreateAimLines()
    {
        Vector2 startPos = transform.position;
        aimLineRen.SetPosition(0, startPos);
        Vector2 raycastVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        for(int x = 0;x <= numberOfBouncesShown;x++)
        {
            RaycastHit2D rayCast = Physics2D.Raycast(startPos, raycastVector, 1000f, indicatorRaycastMask); ;
            if (rayCast.collider != null)
            {
                startPos = rayCast.point - raycastVector.normalized * 0.35f;
                aimLineRen.SetPosition(x+1, startPos);
                raycastVector = Vector2.Reflect(raycastVector, rayCast.normal);//Saves the vector for when it bounces next
            }
        }
    }

    [PunRPC]
    protected override void ToggleOn()
    {
        aimLineRen.enabled = true;
        PlayerShootingScript pShootScript = gameObject.GetComponent<PlayerShootingScript>();
        PlayerMovementScript playerMovementScript = gameObject.GetComponent<PlayerMovementScript>();
        baseReloadSpeed = pShootScript.reloadCooldown;
        baseShootCooldown = pShootScript.shootCooldown;
        playerMovementScript.ChangeMovementBonusRPC(movementBonus);
        pShootScript.reloadCooldown = baseReloadSpeed - reloadBonus;
        pShootScript.shootCooldown = baseShootCooldown - shootCooldownBonus;
        PlayerParticleManager.playerParticleManager.PlayParticle(playerParticleKey + PhotonNetwork.LocalPlayer.NickName);
    }
    [PunRPC]
    protected override void ToggleOff()
    {
        aimLineRen.enabled = false;
        PlayerShootingScript pShootScript = gameObject.GetComponent<PlayerShootingScript>();
        PlayerMovementScript playerMovementScript = gameObject.GetComponent<PlayerMovementScript>();
        playerMovementScript.ChangeMovementBonusRPC(0);
        pShootScript.reloadCooldown = baseReloadSpeed;
        pShootScript.shootCooldown = baseShootCooldown;
        PlayerParticleManager.playerParticleManager.StopParticle(playerParticleKey + PhotonNetwork.LocalPlayer.NickName);
    }
}
