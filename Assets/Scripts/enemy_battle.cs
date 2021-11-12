using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEditor;
using System;
using UnityEngine.SceneManagement;

public class enemy_battle : MonoBehaviour
{
    private Rigidbody2D b;
    private GameObject gun;

    string shopFile = "shop.txt";
    string previousFile = "previouslevel.txt";

    double healthNum;
    double shieldNum;
    double bulletsNum;

    bool isMoving;
    Vector3 original_position;

    bool waiting;
    bool jumpable;
    int enemy = 0;
    public GameObject bulletPrefab;

    private enum EnemyState
    {
        Normal,
        Jumping,
        Crouching,
        Reloading,
        JumpingShooting
    }

    private enum EnemyMode //using enums to represent hierarchical state behaviours of enemy
    {
        Normal,
        Tentative,
        Aggressive,
        Mimic
    }

    [SerializeField]
    Text healthText;

    [SerializeField]
    Text shieldText;

    [SerializeField]
    Text bulletsText;

    [SerializeField]
    Text bulletSpeedText;

    [SerializeField]
    Text playerHealthText;

    [SerializeField]
    Text playerSheildText;

    public Text playerState;

    System.Random rand;

    private EnemyState state = EnemyState.Normal;
    private EnemyMode mode_ = EnemyMode.Normal;

    private void Awake()
    {
        Physics2D.gravity = new Vector2(0.0f, -9.8f);
        isMoving = false;
        original_position = transform.position;
        rand = new System.Random();
        b = transform.GetComponent<Rigidbody2D>();
        using (TextReader file = File.OpenText(shopFile))
        {
            string text = null;
            int i = 0;
            while ((text = file.ReadLine()) != null)
            {
                if (i == 4)
                {
                    enemy = int.Parse(text);
                    break;
                }
                i++;
            }
        }
        gun = GameObject.Find("gun1");
        healthNum = 100;
        shieldNum = 0;
        if (enemy == 1)
        {
            bulletsNum = 10; //if user has bought 'less space in magazine' powerup from shop
        }
        else
        {
            bulletsNum = 20; //if user hasnt bought the powerup
        }
        healthText.text = "Health: " + healthNum;
        shieldText.text = "Shield: " + shieldNum;
        bulletsText.text = "Bullets: " + bulletsNum;

        waiting = false;
        jumpable = true;
    }

    private void Update()
    {
        Debug.Log(mode_);
        if (playerState.text != "Won" && playerState.text != "Lost")
        {
            string[] splits = playerHealthText.text.Split(' ');
            string[] splits1 = playerSheildText.text.Split(' ');
            if (healthNum <= 15) //defines hierarchical state machine - enemy has mode and states within the mode
            {
                mode_ = EnemyMode.Tentative;
            } else if (int.Parse(splits[1]) <= 15){
                int roll1 = rand.Next(1, 5001);
                int roll2 = rand.Next(1, 5001); //a chance for AI to go 'aggressive' for a brief period of time 
                if (roll1 == roll2)
                {
                    if (mode_ != EnemyMode.Aggressive)
                    {
                        mode_ = EnemyMode.Aggressive;
                        StartCoroutine(backToNormalMode(5.0f)); //back to normal mode
                    }
                }
            }
            else if ((int.Parse(splits[1]) + int.Parse(splits1[1])) == healthNum)
            {
                mode_ = EnemyMode.Mimic; //if health levels are identical, mimic mode
            }
            else
            {
                mode_ = EnemyMode.Normal; //else, 'normal' mode but checks continue for reason to enter other modes
            }

            if (state == EnemyState.Normal)
            {
                var scale = transform.localScale;
                scale.x = 1f;
                scale.y = 1f;
                transform.localScale = scale;
                GetComponent<CircleCollider2D>().radius = 0.5f;
            }


            gun.transform.position = new Vector3(transform.position.x - 0.51f, transform.position.y - 0.1429856f, transform.position.z - 1f);

            if(mode_ != EnemyMode.Mimic)
            {
                if (state != EnemyState.Jumping && state != EnemyState.JumpingShooting && state != EnemyState.Reloading && state != EnemyState.Crouching && jumpable)
                {
                    if (playerState.text == "Jumping" || playerState.text == "Shooting" || playerState.text == "Normal")
                    {
                        int roll1;
                        int roll2;
                        if(mode_ != EnemyMode.Tentative)
                        {
                            roll1 = rand.Next(1, 251);
                            roll2 = rand.Next(1, 251);
                        }
                        else
                        {
                            roll1 = rand.Next(1, 1501);
                            roll2 = rand.Next(1, 1501); //enemy much less likely to jump and become 'exposed' in tentative mode
                        }
                        if (roll1 == roll2 || mode_ == EnemyMode.Aggressive) //enemy will constantly jump and fire if set to 'aggressive' mode
                        {
                            float jump = 5f;
                            b.velocity = Vector2.up * jump;
                            if (state == EnemyState.Normal)
                            {
                                state = EnemyState.Jumping;
                                jumpable = false;
                            }
                        }
                    }
                }
            }
            else
            {
                StartCoroutine(delay(0.4f));
                if (playerState.text == "Normal")
                {
                    state = EnemyState.Normal;
                } else if ((playerState.text == "Jumping" || playerState.text == "Shooting") && jumpable)
                {
                    float jump = 5f;
                    b.velocity = Vector2.up * jump;
                    if (state == EnemyState.Normal)
                    {
                        state = EnemyState.Jumping;
                        jumpable = false;
                    }
                } else if (playerState.text == "Reloading" || playerState.text == "Crouching")
                {
                    state = EnemyState.Crouching;
                    StartCoroutine(takeCover(0.0f)); //mimics player behaviour almost exactly while in mimic mode
                }
            }

            moving();
        }
        else
        {
            gun.transform.position = new Vector3(transform.position.x - 0.51f, transform.position.y - 0.1429856f, transform.position.z - 1f);

            File.Create(previousFile).Close();
            if (Input.GetKeyDown(KeyCode.Return))
            {
                using (StreamWriter sw = File.AppendText(previousFile))
                {
                    if (playerState.text == "Won")
                    {
                        sw.Write("level2win"); //to take user back to the right icon when loading scene
                    }
                    else
                    {
                        sw.Write("level2lose");
                    }
                }
                SceneManager.LoadScene("Overworld");
            }
        }
    }

    private void moving()
    {
        if (bulletsNum > 0 && (state == EnemyState.Jumping || state == EnemyState.JumpingShooting) && !waiting && playerState.text != "Reloading" && playerState.text != "Crouching" && gun.transform.position.y > 119f)
        {
            bool cont = true;
            if (mode_ == EnemyMode.Mimic && playerState.text != "Shooting")
            {
                cont = false; //if in mimic mode it only shoots when player shoots, rare occurrence after start that health levels match
            }
            if (cont)
            {
                int roll1 = rand.Next(1, 26);
                int roll2 = rand.Next(1, 26);
                if (roll1 == roll2) //so the enemy doesnt shoot at the exact same y positions every time it jumps
                {
                    state = EnemyState.JumpingShooting;
                    GameObject bullet = Instantiate(bulletPrefab, gun.transform.position, Quaternion.identity);
                    bullet.GetComponent<Rigidbody2D>().gravityScale = 0.0f;
                    bullet.transform.Rotate(0f, 180f, 0f, Space.Self);
                    double min = 15;
                    double max = 25;
                    double range = max - min;
                    double sample = rand.NextDouble();
                    double scaled = (sample * range) + min;
                    float speedFloat = (float)scaled; //bullet random speed to do random amount of damage upon collision with enemy
                    bullet.GetComponent<Rigidbody2D>().velocity = new Vector3(-speedFloat, 0f, 0f);
                    bulletsNum -= 1;
                    string rounded = String.Format("{0:0.00}", speedFloat);
                    bulletSpeedText.text = "Bullet Speed: " + rounded;
                    if (bulletsNum > 0)
                    {
                        bulletsText.text = "Bullets: " + bulletsNum;
                    }
                    else
                    {
                        bulletsText.text = "RELOAD";
                    }
                    if (mode_ != EnemyMode.Aggressive)
                    {
                        StartCoroutine(normalState(0.4f, "fire"));
                    }
                    else
                    {
                        StartCoroutine(normalState(0.25f, "fire")); //faster cooldown between shots for aggressive mode
                    }
                }
            }
        }

        if (state != EnemyState.Jumping)
        {
            if (state != EnemyState.Reloading)
            {
                if (bulletsNum == 0)
                {
                    state = EnemyState.Reloading;
                    bulletsText.text = "Reloading...";
                    if(mode_ == EnemyMode.Tentative)
                    {
                        StartCoroutine(takeCover(0.05f)); //enemy will crouch and reload at the same time only in 'tentative' mode
                    }
                    StartCoroutine(normalState(2.5f, "bullets"));
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(playerState.text != "Won" && playerState.text != "Lost")
        {
            if (collision.gameObject.CompareTag("Platform"))
            {
                jumpable = true;
                if (state != EnemyState.Crouching)
                {
                    state = EnemyState.Normal;
                }
            }

            if (collision.gameObject.CompareTag("BulletPlayer"))
            {
                float damage = (collision.gameObject.GetComponent<Rigidbody2D>().velocity.x) / 2.5f;
                Destroy(collision.gameObject);
                double rounded = Math.Round(damage, 0, MidpointRounding.AwayFromZero);
                rounded = Math.Abs(rounded);
                if (shieldNum > rounded)
                {
                    shieldNum -= rounded;
                }
                else if (shieldNum > 0 && shieldNum < rounded)
                {
                    double remainder = rounded - shieldNum;
                    shieldNum = 0;
                    healthNum -= remainder;
                }
                else if (shieldNum == 0)
                {
                    if (healthNum > rounded)
                    {
                        healthNum -= rounded;
                    }
                    else
                    {
                        healthNum = 0;
                    }
                } //calculates damage done to enemy based on speed of bullet

                healthText.text = "Health: " + healthNum;
                shieldText.text = "Shield: " + shieldNum;

                StartCoroutine(backToPosition(0.2f));
                if(mode_ != EnemyMode.Tentative)
                {
                    StartCoroutine(takeCover(1.25f));
                }
                else
                {
                    StartCoroutine(takeCover(0.25f)); //player will duck when hit, reacts faster in tentative mode
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(playerState.text != "Won" && playerState.text != "Lost")
        {
            if (collision.gameObject.CompareTag("Platform"))
            {
                if (state != EnemyState.Crouching)
                {
                    state = EnemyState.Jumping;
                    jumpable = false;
                }
            }
        }
    }

    private IEnumerator takeCover(float wait)
    {
        yield return new WaitForSeconds(wait);
        state = EnemyState.Crouching;
        var scale = transform.localScale;
        scale.x = 0.8f;
        scale.y = 0.8f;
        transform.localScale = scale;
        GetComponent<CircleCollider2D>().radius = 0.4f;
        StartCoroutine(normalState(3.5f, "crouch")); //visual aid to show crouching, then back to normal
    }

    private IEnumerator normalState(float wait, string mode)
    {
        waiting = true;
        yield return new WaitForSeconds(wait);
        waiting = false;
        if (mode == "bullets")
        {
            if(enemy == 1)
            {
                bulletsNum = 10;
            }
            else
            {
                bulletsNum = 20;
            }
            bulletsText.text = "Bullets: " + bulletsNum;
        }
        if(playerState.text != "Won" && playerState.text != "Lost")
        {
            if (mode == "fire" && state != EnemyState.Normal)
            {
                state = EnemyState.Jumping;
            }
            else
            {
                state = EnemyState.Normal; //back to previous mode
            }
        }
    }

    private IEnumerator backToPosition(float wait)
    {
        yield return new WaitForSeconds(wait);
        b.velocity = new Vector2(0f, 0f);
        b.angularVelocity = 0.0f;
        StartCoroutine(moveEnemy(this.transform, original_position, 0.2f)); //move back to set position on screen after being hit by bullet
    }

    private IEnumerator backToNormalMode(float wait)
    {
        yield return new WaitForSeconds(wait);
        mode_ = EnemyMode.Normal;
    }

    private IEnumerator delay(float wait)
    {
        yield return new WaitForSeconds(wait);
    }

    IEnumerator moveEnemy(Transform fromPosition, Vector3 toPosition, float duration)
    {
        if (isMoving)
        {
            yield break;
        }
        isMoving = true;

        float counter = 0;

        Vector3 startPos = fromPosition.position;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            fromPosition.position = Vector3.Lerp(startPos, toPosition, counter / duration);
            yield return null;
        }

        isMoving = false; //code to move enemy back to required position using Vector3.Lerp
    }
}
