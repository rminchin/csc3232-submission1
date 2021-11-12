using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor;
using System;

public class player_battle : MonoBehaviour
{
    private Rigidbody2D b;
    private GameObject gun;
    public GameObject gun1;
    public GameObject enemy;

    string shopFile = "shop.txt";
    string previousFile = "previouslevel.txt";
    bool ended;

    double healthNum;
    double shieldNum;
    double bulletsNum;

    private GameObject winUI;
    private GameObject loseUI;

    Vector3 original_position;
    bool isMoving;

    int health = 0;
    int bullets = 0;

    public GameObject bulletPrefab;

    private enum PlayerState //state based behaviour for player controlled by user as well
    {
        Normal,
        Jumping,
        Shooting,
        Crouching,
        Reloading,
        Won,
        Lost
    }

    [SerializeField]
    Text healthText;

    [SerializeField]
    Text shieldText;

    [SerializeField]
    Text bulletsText;

    [SerializeField]
    Text bulletSpeedText;

    public Text stateText;
    public Text EnemyHealthText;

    private PlayerState state = PlayerState.Normal;

    private void Awake()
    {
        Physics2D.gravity = new Vector2(0.0f, -9.8f);
        ended = false;
        original_position = transform.position;
        isMoving = false;
        b = transform.GetComponent<Rigidbody2D>();
        using (TextReader file = File.OpenText(shopFile))
        {
            string text = null;
            int i = 0;
            while ((text = file.ReadLine()) != null)
            {
                if (i == 2)
                {
                    health = int.Parse(text);
                } else if (i == 3)
                {
                    bullets = int.Parse(text);
                    break;
                }
                i++;
            }
        }
        gun = GameObject.Find("gun");
        healthNum = 100;
        if(health == 1)
        {
            shieldNum = 50;
        }
        else
        {
            shieldNum = 0; //finds which powerups have been bought in the shop and sets variables accordingly
        }
        bulletsNum = 20;
        healthText.text = "Health: " + healthNum;
        shieldText.text = "Shield: " + shieldNum;
        bulletsText.text = "Bullets: " + bulletsNum;

        winUI = GameObject.Find("WinUI");
        winUI.SetActive(false);
        loseUI = GameObject.Find("LoseUI");
        loseUI.SetActive(false);
    }

    private void Update()
    {
        if(healthNum == 0)
        {
            state = PlayerState.Lost;
            healthText.text = "Health: 0";
        } else if (EnemyHealthText.text == "Health: 0")
        {
            state = PlayerState.Won; //check if player has won or lost
        }

        stateText.text = "" + state;

        if(state == PlayerState.Won || state == PlayerState.Lost)
        {
            finalEdits();
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SceneManager.LoadScene("Overworld"); //returns to overworld
            }
        }
        else
        {
            stateText.text = "" + state;
            if (state == PlayerState.Normal)
            {
                var scale = transform.localScale;
                scale.x = 1f;
                scale.y = 1f;
                transform.localScale = scale;
                GetComponent<CircleCollider2D>().radius = 0.5f;
            }

            gun.transform.position = new Vector3(transform.position.x + 0.51f, transform.position.y - 0.1429856f, transform.position.z - 1.0f);

            if (state != PlayerState.Jumping && state != PlayerState.Reloading && state != PlayerState.Crouching && Input.GetKeyDown(KeyCode.Space))
            {
                float jump = 5f;
                b.velocity = Vector2.up * jump;
                state = PlayerState.Jumping;
            }

            moving();
        }
    }

    private void finalEdits()
    {
        if (!ended)
        {
            ended = true;

            GameObject[] bulletsAI = GameObject.FindGameObjectsWithTag("BulletAI");
            GameObject[] bulletsPlayer = GameObject.FindGameObjectsWithTag("BulletPlayer");

            foreach (GameObject o in bulletsAI)
            {
                Destroy(o);
            }
            foreach (GameObject o in bulletsPlayer)
            {
                Destroy(o); //destroys all bullets on screen when a player/enemy hits 0 health
            }

            if (state == PlayerState.Won)
            {
                Destroy(enemy.gameObject);
                Destroy(gun1.gameObject);
                gun.transform.position = new Vector3(transform.position.x + 0.51f, transform.position.y - 0.1429856f, transform.position.z - 1.0f);
                winUI.SetActive(true);
            }
            else
            {
                Destroy(gun.gameObject);
                gun1.transform.position = new Vector3(enemy.transform.position.x - 0.51f, enemy.transform.position.y - 0.1429856f, enemy.transform.position.z - 1f);
                loseUI.SetActive(true);
                Destroy(this.gameObject);
            }
        }

        File.Create(previousFile).Close();
        if (Input.GetKeyDown(KeyCode.Return))
        {
            using (StreamWriter sw = File.AppendText(previousFile))
            {
                if(state == PlayerState.Won)
                {
                    sw.Write("level2win");
                }
                else
                {
                    sw.Write("level2lose"); //edits file accordingly so user goes back to overworld at the right icon
                }
            }
        }
    }

    private void moving()
    {
        if(state != PlayerState.Reloading && state != PlayerState.Crouching && state != PlayerState.Shooting)
        {
            if (Input.GetKeyDown(KeyCode.F) && bulletsNum > 0)
            {
                state = PlayerState.Shooting;
                GameObject bullet = Instantiate(bulletPrefab, gun.transform.position, Quaternion.identity);
                bullet.GetComponent<Rigidbody2D>().gravityScale = 0.0f;
                System.Random rand = new System.Random();
                double min;
                double max;
                double range;
                if (bullets == 1)
                {
                    min = 25;
                    max = 35;
                }
                else
                {
                    min = 15;
                    max = 25;
                }
                range = max - min;
                double sample = rand.NextDouble();
                double scaled = (sample * range) + min;
                float speedFloat = (float)scaled; //bullet random speed to do random amount of damage upon collision with enemy
                bullet.GetComponent<Rigidbody2D>().velocity = new Vector3(speedFloat, 0f, 0f);
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
                StartCoroutine(normalState(0.5f, "fire"));
            }
        }

        if(state != PlayerState.Jumping)
        {
            if(state != PlayerState.Crouching)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    state = PlayerState.Crouching;
                    var scale = transform.localScale;
                    scale.x = 0.8f;
                    scale.y = 0.8f;
                    transform.localScale = scale;
                    GetComponent<CircleCollider2D>().radius = 0.4f;
                    StartCoroutine(normalState(3.5f, "crouch")); //code to visually enter crouch state
                }
            }

            if(state != PlayerState.Reloading)
            {
                if (Input.GetKeyDown(KeyCode.R) && bulletsNum == 0)
                {
                    state = PlayerState.Reloading;
                    bulletsText.text = "Reloading...";
                    StartCoroutine(normalState(2.5f, "bullets"));
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(state != PlayerState.Won && state != PlayerState.Lost)
        {
            if (collision.gameObject.CompareTag("Platform"))
            {
                if (state != PlayerState.Crouching)
                {
                    state = PlayerState.Normal;
                }
            }

            if (collision.gameObject.CompareTag("BulletAI"))
            {
                float damage = (collision.gameObject.GetComponent<Rigidbody2D>().velocity.x) / 2.5f;
                Destroy(collision.gameObject);
                double rounded = Math.Round(damage, 0, MidpointRounding.AwayFromZero);
                rounded = Math.Abs(rounded); //damage calculated based on speed of the bullet upon collision
                if (shieldNum > rounded)
                {
                    shieldNum -= rounded;
                }
                else if (shieldNum > 0 && shieldNum <= rounded)
                {
                    double remainder = rounded - shieldNum;
                    shieldNum = 0;
                    healthNum -= remainder;
                } else if (shieldNum == 0)
                {
                    if (healthNum > rounded)
                    {
                        healthNum -= rounded;
                    }
                    else
                    {
                        state = PlayerState.Lost;
                    }
                }

                healthText.text = "Health: " + healthNum;
                shieldText.text = "Shield: " + shieldNum;

                StartCoroutine(backToPosition(0.2f)); //moves player back to desired position after bullet collision
            }
        }
    }

    private IEnumerator normalState(float wait, string mode)
    {
        yield return new WaitForSeconds(wait);
        if (mode == "bullets")
        {
            bulletsNum = 20;
            bulletsText.text = "Bullets: " + bulletsNum;
        }
        if (state != PlayerState.Won && state != PlayerState.Lost)
        {
            state = PlayerState.Normal;
        }
    }

    private IEnumerator backToPosition(float wait)
    {
        yield return new WaitForSeconds(wait);
        b.velocity = new Vector2(0f, 0f);
        b.angularVelocity = 0.0f;
        StartCoroutine(movePlayer(this.transform, original_position, 0.2f));
    }

    IEnumerator movePlayer(Transform fromPosition, Vector3 toPosition, float duration)
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

        isMoving = false;
    }
}