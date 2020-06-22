﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShakeScript : MonoBehaviour
{
    private float shakeForce = 80f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 10)//Checks if is projectile
        {
            Vector2 dir = collision.gameObject.GetComponent<Rigidbody2D>().velocity.normalized;
            CameraScript.cameraScript.CameraShake(dir,shakeForce);
        }
    }
}
