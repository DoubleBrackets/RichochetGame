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

    private static int projectileViewId = 50;


    private float shootCooldown = 0.3f;
    private float shootCooldownTimer = 0;

    private int ammo = 3;
    private int maxAmmo = 3;

    private float reloadCooldown = 2f;
    private float reloadCooldownTimer = 0;

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
            if(reloadCooldownTimer <= 0)
            {
                ammo = maxAmmo;
                gameObject.GetComponent<PlayerNetworkingScript>().SetAmmoValue(ammo, maxAmmo);
            }
        }
        if (Input.GetMouseButtonDown(0) && shootCooldownTimer <= 0 && ammo > 0)
        {
            ammo--;
            //Updates ammo value
            gameObject.GetComponent<PlayerNetworkingScript>().SetAmmoValue(ammo,maxAmmo);
            if (ammo == 0)
            {
                reloadCooldownTimer = reloadCooldown;
            }
            shootCooldownTimer = shootCooldown;
            Vector2 dir = GetMouseDir();
            photonView.RPC("CreateProjectile", RpcTarget.All, transform.position + (Vector3)dir.normalized * offSet,speed,(Vector3)dir,photonView.ViewID,(float)PhotonNetwork.Time);
        }
    }

    [PunRPC]
    public void CreateProjectile(Vector3 pos, float speed, Vector3 dir,int shooterViewId,float rpcInvokeTime)
    {
        GameObject proj = Instantiate(projPrefab, pos, Quaternion.identity);
        proj.GetPhotonView().ViewID = projectileViewId;
        ProjectileScript pScript = proj.GetComponent<ProjectileScript>();
        pScript.speed = speed;
        //Lag compensation
        Vector3 predictedPos = proj.transform.position + dir * Mathf.Max(0,(float)(PhotonNetwork.Time - rpcInvokeTime)/2f);

        pScript.OnBulletBounce(predictedPos,dir,0,(float)PhotonNetwork.Time);
        projectileViewId++;


        
        //Disables collisions with the shooter for a period of time
        Collider2D shooterColl = PhotonNetwork.GetPhotonView(shooterViewId).gameObject.GetComponent<Collider2D>();
        Collider2D projColl = proj.GetComponent<Collider2D>();
        StartCoroutine(StopIgnoringCollision(shooterColl,projColl));

    }
    IEnumerator StopIgnoringCollision(Collider2D c1, Collider2D c2)
    {
        Physics2D.IgnoreCollision(c1, c2,true);
        yield return new WaitForSeconds(0.2f);
        Physics2D.IgnoreCollision(c1, c2,false);
    }

    private Vector2 GetMouseDir()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return (mousePos - (Vector2)transform.position);
        
    }

}
