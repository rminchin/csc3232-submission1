using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class bullet : MonoBehaviour
{

    System.Random rand;

    private void Awake()
    {
        rand = new System.Random();
    }

    private void OnBecameInvisible()
    {
        Destroy(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            Destroy(this.gameObject);
        }

        if(collision.gameObject.CompareTag("BulletPlayer") || collision.gameObject.CompareTag("BulletAI")){
            Rigidbody2D thisBullet = transform.GetComponent<Rigidbody2D>();
            Rigidbody2D otherBullet = collision.gameObject.GetComponent<Rigidbody2D>();

            double min = -15f;
            double max = 15f;
            double range = max - min;
            double sample = rand.NextDouble();
            double sample1 = rand.NextDouble();
            double scaled = (sample * range) + min;
            double scaled1 = (sample1 * range) + min;
            float randSpeed1 = (float)scaled;
            float randSpeed2 = (float)scaled1; //bullets given random y velocity component when colliding with other bullets

            thisBullet.velocity = new Vector2(thisBullet.velocity.x, randSpeed1);
            otherBullet.velocity = new Vector2(otherBullet.velocity.x, randSpeed2);

            Physics2D.IgnoreCollision(transform.GetComponent<Collider2D>(), collision.gameObject.GetComponent<Collider2D>());
        }
    }
}
