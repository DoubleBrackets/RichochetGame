using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerShootingScript : MonoBehaviourPunCallbacks
{
    private float offSet = 0f;

    private float speed = 80;

    public GameObject projPrefab;
    public GameObject muzzleFlashObject;

    public Collider2D screenShakeTrigger;

    private float shootCooldown = 0.2f;
    private float shootCooldownTimer = 0;

    private int ammo = 3;
    private int maxAmmo = 3;

    private float reloadCooldown = 2f;
    private float reloadCooldownTimer = 0;

    private float recoilForce = 35f;

    void Update()
    {
        if (!photonView.IsMine || !NetworkManager.networkManager.gameStarted)
            return;
        if (shootCooldownTimer > 0)
        {
            shootCooldownTimer -= Time.deltaTime;
        }
        if (reloadCooldownTimer > 0)
        {
            reloadCooldownTimer -= Time.deltaTime;
            gameObject.GetComponent<PlayerNetworkingScript>().UpdateReloadBarRPC(reloadCooldownTimer, reloadCooldown);
            if (reloadCooldownTimer <= 0)
            {
                ammo = maxAmmo;
                gameObject.GetComponent<PlayerNetworkingScript>().SetAmmoValueRPC(ammo, maxAmmo);
            }
        }
        if (Input.GetMouseButtonDown(0) && shootCooldownTimer <= 0 && ammo > 0)
        {
            ammo--;
            //Updates ammo value
            gameObject.GetComponent<PlayerNetworkingScript>().SetAmmoValueRPC(ammo,maxAmmo);
            if (ammo == 0)
            {
                reloadCooldownTimer = reloadCooldown;
            }
            shootCooldownTimer = shootCooldown;
            ShootProjectile();
            
        }
        if(Input.GetKeyDown(KeyCode.R) && ammo != maxAmmo && reloadCooldownTimer <= 0)
        {
            ammo = 0;
            reloadCooldownTimer = reloadCooldown;
        }
    }

    public void ResetAmmo()
    {
        ammo = maxAmmo;
        gameObject.GetComponent<PlayerNetworkingScript>().SetAmmoValueRPC(ammo, maxAmmo);
    }

    private void ShootProjectile()
    {
        Vector2 dir = GetMouseDir();
        //Screen jerk recoil
        CameraScript.cameraScript.CameraShake(-dir, recoilForce);

        //Creates local projectile first
        GameObject newProj = CreateProjectile(transform.position + (Vector3)dir.normalized * offSet, speed, dir);
        PhotonView pView = newProj.GetPhotonView();
        PhotonNetwork.AllocateViewID(pView);
        //Sends rpc to other client to create projectile
        photonView.RPC("CreateProjectile", RpcTarget.Others, transform.position + (Vector3)dir.normalized * offSet, speed, (Vector3)dir, photonView.ViewID, pView.ViewID,(float)PhotonNetwork.Time,PhotonNetwork.LocalPlayer.NickName);
    }

    //Instantiating local projectile
    public GameObject CreateProjectile(Vector3 pos, float speed, Vector3 dir)
    {
        GameObject proj = Instantiate(projPrefab, pos, Quaternion.identity);
        ProjectileScript pScript = proj.GetComponent<ProjectileScript>();
        pScript.speed = speed;
        //Particles
        float angle = Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x);
        muzzleFlashObject.transform.rotation = Quaternion.Euler(0, 0, angle);
        PlayerParticleManager.playerParticleManager.PlayParticleRPC("MuzzleFlash" + PhotonNetwork.LocalPlayer.NickName);

        pScript.OnBulletBounce(pos, dir, 0, (float)PhotonNetwork.Time);
        //Disables collisions with the shooter for a period of time
        Collider2D shooterColl = gameObject.GetComponent<Collider2D>();
        Collider2D projColl = proj.GetComponent<Collider2D>();
        StartCoroutine(StopIgnoringCollision(shooterColl, projColl));

        return proj;

    }
    //Instantiating through RPC
    [PunRPC]
    public GameObject CreateProjectile(Vector3 pos, float speed, Vector3 dir,int shooterViewId,int projViewId,float rpcInvokeTime,string sourceNickname)
    {
        
        GameObject proj = Instantiate(projPrefab, pos, Quaternion.identity);
        ProjectileScript pScript = proj.GetComponent<ProjectileScript>();
        pScript.speed = speed;
        proj.GetPhotonView().ViewID = projViewId;
        //Lag compensation
        Vector3 predictedPos = proj.transform.position + dir * Mathf.Max(0,(float)(PhotonNetwork.Time - rpcInvokeTime)/2f);

        pScript.OnBulletBounce(predictedPos,dir,0,(float)PhotonNetwork.Time);
        //Particles
        float angle = Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x);
        muzzleFlashObject.transform.rotation = Quaternion.Euler(0, 0, angle);
        PlayerParticleManager.playerParticleManager.PlayParticleRPC("MuzzleFlash" + sourceNickname);


        //Disables collisions with the shooter for a period of time
        Collider2D shooterColl = PhotonNetwork.GetPhotonView(shooterViewId).gameObject.GetComponent<Collider2D>();
        Collider2D projColl = proj.GetComponent<Collider2D>();
        StartCoroutine(StopIgnoringCollision(shooterColl,projColl));

        return proj;

    }
    IEnumerator StopIgnoringCollision(Collider2D c1, Collider2D proj)
    {
        Physics2D.IgnoreCollision(c1, proj, true);
        Physics2D.IgnoreCollision(screenShakeTrigger, proj, true);
        yield return new WaitForSeconds(0.2f);
        Physics2D.IgnoreCollision(c1, proj, false);
        Physics2D.IgnoreCollision(screenShakeTrigger, proj, false);
    }

    private Vector2 GetMouseDir()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return (mousePos - (Vector2)transform.position);
        
    }

}
