using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerShootingScript : MonoBehaviourPunCallbacks
{
    private float offSet = 2f;

    private float speed = 80;

    public GameObject projPrefab;

    private static int projectileViewId = 50;

    void Update()
    {
        if (!photonView.IsMine)
            return;
        if(Input.GetMouseButtonDown(0))
        {
            Vector2 dir = GetMouseDir();
            photonView.RPC("CreateProjectile", RpcTarget.AllViaServer, transform.position + (Vector3)dir.normalized * offSet,speed,(Vector3)dir,photonView.ViewID);
        }
    }

    [PunRPC]
    public void CreateProjectile(Vector3 pos, float speed, Vector3 dir,int shooterViewId)
    {
        GameObject proj = Instantiate(projPrefab, pos, Quaternion.identity);
        proj.GetPhotonView().ViewID = projectileViewId;
        ProjectileScript pScript = proj.GetComponent<ProjectileScript>();
        pScript.speed = speed;
        pScript.OnBulletShoot(pos,0, 0, dir.normalized * speed);
        projectileViewId++;
        //Disables collisions with the shooter for a period of time
        Collider2D shooterColl = PhotonNetwork.GetPhotonView(shooterViewId).gameObject.GetComponent<Collider2D>();
        Collider2D projColl = proj.GetComponent<Collider2D>();
        StartCoroutine(StopIgnoringCollision(shooterColl,projColl));

    }
    IEnumerator StopIgnoringCollision(Collider2D c1, Collider2D c2)
    {
        Physics2D.IgnoreCollision(c1, c2,true);
        yield return new WaitForSeconds(1.5f);
        Physics2D.IgnoreCollision(c1, c2,false);
    }

    private Vector2 GetMouseDir()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return (mousePos - (Vector2)transform.position);
        
    }
}
