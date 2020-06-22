using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationScript : MonoBehaviour
{
    PlayerMovementScript pMoveScript;
    Animator anim;
    Rigidbody2D playerRb;
    SpriteRenderer spriteRen;

    public GameObject dashAftereffect;

    private int horizontalInput;
    private int verticalInput;
    private bool isDashing;
    private float dashingHorizontalComponent;
    private float dashingVerticalComponent;

    //dash aftereffect
    private float afterEffectCooldown = 0.12f;
    private float afterEffectTimer = 0;

    private void Awake()
    {
        pMoveScript = gameObject.GetComponent<PlayerMovementScript>();
        playerRb = gameObject.GetComponent<Rigidbody2D>();
        anim = gameObject.GetComponent<Animator>();
        spriteRen = gameObject.GetComponent<SpriteRenderer>();
    }


    private void Update()
    {
        if(afterEffectTimer > 0)
        {
            afterEffectTimer -= Time.deltaTime;
        }
        horizontalInput = pMoveScript.GetHorizontalInput();
        verticalInput = pMoveScript.GetVerticalInput();
        isDashing = pMoveScript.GetIsDashing();
        dashingHorizontalComponent = pMoveScript.GetDashingHorizontal();
        dashingVerticalComponent = pMoveScript.GetDashingVertical();

        anim.SetBool("IsDashing", isDashing);
        if (isDashing)
        {
            if(afterEffectTimer <= 0)
                CreateDashAftereffect();

            int dashingHComponent = Mathf.RoundToInt(dashingHorizontalComponent);
            int dashingVComponent = Mathf.RoundToInt(dashingVerticalComponent);           
            anim.SetInteger("DashingVerticalValue", dashingVComponent);
            anim.SetInteger("VerticalInput", dashingVComponent);
            if (dashingHComponent == 0)
            {
                anim.SetBool("IsDashingHorizontal", false);
                anim.SetBool("HorizontalInputActive", false);
            }
            else if (dashingHComponent == 1)
            {
                anim.SetBool("IsDashingHorizontal", true);
                anim.SetBool("HorizontalInputActive", true);
                spriteRen.flipX = false;
            }
            else if (dashingHComponent == -1)
            {
                anim.SetBool("IsDashingHorizontal", true);
                anim.SetBool("HorizontalInputActive", true);
                spriteRen.flipX = true;
            }
        }
        else
        {
            if (playerRb.velocity.magnitude <= 0.1f)//If not moving, then dont play any animations(e.g , running into a wall)
            {
                anim.SetBool("HorizontalInputActive", false);
                anim.SetInteger("VerticalInput", 0);
                return;
            }


            anim.SetInteger("VerticalInput", verticalInput);

            if (horizontalInput == 0)
            {
                anim.SetBool("HorizontalInputActive", false);
            }
            else if (horizontalInput == 1)
            {
                anim.SetBool("HorizontalInputActive", true);
                spriteRen.flipX = false;
            }
            else if (horizontalInput == -1)
            {
                anim.SetBool("HorizontalInputActive", true);
                spriteRen.flipX = true;
            }
        }
    }

    public void CreateDashAftereffect()
    {
        afterEffectTimer = afterEffectCooldown;
        GameObject g = Instantiate(dashAftereffect, transform.position, Quaternion.identity);
        SpriteRenderer ren = g.GetComponent<SpriteRenderer>();
        ren.sprite = spriteRen.sprite;
        ren.flipX = spriteRen.flipX;
    }
}
