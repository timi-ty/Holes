﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    private float bulletSpeed;
    private ParticleSystem hitSpark;
    private void Start()
    {
        Disable();
    }

    void Update()
    {
        transform.position += transform.right * Time.deltaTime * bulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.rigidbody && collision.rigidbody.bodyType == RigidbodyType2D.Dynamic && collision.rigidbody.CompareTag("Obstacle"))
        {
            collision.rigidbody.AddForce(Vector2.right * bulletSpeed/4, ForceMode2D.Impulse);
        }
        if (hitSpark)
        {
            hitSpark.transform.position = transform.position;
            hitSpark.Play();
        }
        Disable();
    }

    public void Shoot(Vector3 position, Quaternion rotation, float bulletSpeed, ParticleSystem hitSpark)
    {
        transform.SetPositionAndRotation(position, rotation);
        this.bulletSpeed = bulletSpeed;
        this.hitSpark = hitSpark;

        Invoke("Disable", 2);
    }

    private void Disable()
    {
        gameObject.SetActive(false);
    }
}
