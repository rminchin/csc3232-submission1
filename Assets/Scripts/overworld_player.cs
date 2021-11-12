//code for a simple overworld system, user interacts with this to go to 'encounters' - level 1, shop, battle

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class overworld_player : MonoBehaviour
{
    private enum Playerstate
    {
        Level0,
        Level1,
        Level2,
        Shop,
        Level3
    }

    private GameObject error1;
    private GameObject error2;
    private GameObject error3;
    private GameObject error4;
    private GameObject error5;

    private GameObject level1icon;
    private GameObject shopicon;
    private GameObject homeicon;
    private GameObject level2icon;

    private bool moving = false;

    private bool left = false;
    private bool right = true;
    private bool level2 = false;
    private bool level3 = false;

    private bool error = false;

    private string level1stars = "stars-level1.txt";
    private string previouslevel = "previouslevel.txt";

    private Playerstate state = Playerstate.Level0;
    private int starscollected;

    void Awake()
    {
        Physics2D.gravity = new Vector2(0f, 0f);
        error1 = GameObject.Find("WrongInput1");
        error1.SetActive(false);
        error2 = GameObject.Find("WrongInput2");
        error2.SetActive(false);
        error3 = GameObject.Find("WrongInput3");
        error3.SetActive(false);
        error4 = GameObject.Find("WrongInput4");
        error4.SetActive(false);
        error5 = GameObject.Find("LevelDoesntExist1");
        error5.SetActive(false);

        level1icon = GameObject.Find("level1");
        shopicon = GameObject.Find("shop");
        homeicon = GameObject.Find("level0");
        level2icon = GameObject.Find("level2");
        starscollected = 0;

        using (TextReader file = File.OpenText(level1stars))
        {
            string text = null;
            int i = 0;
            while ((text = file.ReadLine()) != null)
            {
                if (i == 1)
                {
                    starscollected = int.Parse(text);
                }
                i++;
            }
        }

        if (starscollected > 0)
        {
            level2 = true;
        }

        using (TextReader file = File.OpenText(previouslevel))
        {
            string text = null;
            while ((text = file.ReadLine()) != null)
            {
                if(text == "level1")
                {
                    level2 = true;
                    transform.position = new Vector3(level1icon.transform.position.x, level1icon.transform.position.y + 5.8f, level1icon.transform.position.z - 0.95f);
                } else if(text == "shop")
                {
                    level2 = true;
                    transform.position = new Vector3(shopicon.transform.position.x, shopicon.transform.position.y + 5.8f, shopicon.transform.position.z - 0.95f);
                } else if(text == "menu")
                {
                    transform.position = new Vector3(homeicon.transform.position.x, homeicon.transform.position.y + 5.8f, homeicon.transform.position.z - 0.95f);
                } else if(text == "level2win" || text == "level2lose")
                {
                    level2 = true;
                    if(text == "level2win")
                    {
                        level3 = true;
                    }
                    transform.position = new Vector3(level2icon.transform.position.x, level2icon.transform.position.y + 5.8f, level2icon.transform.position.z - 0.95f);
                }
            }
        }
    }

    void Update()
    {
        if (!moving && !error)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (left)
                {
                    move(true);
                }
                else
                {
                    error = true;
                    error3.SetActive(true);
                    StartCoroutine(errorMessage(error3));
                }
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                if (right)
                {
                    move(false);
                }
                else
                {
                    error = true;
                    error4.SetActive(true);
                    StartCoroutine(errorMessage(error4));
                }
            } else if (Input.GetKeyDown(KeyCode.Return))
            {
                if (state == Playerstate.Level1)
                {
                    SceneManager.LoadScene("LevelOne");
                } else if (state == Playerstate.Shop)
                {
                    SceneManager.LoadScene("Shop");
                } else if (state == Playerstate.Level2)
                {
                    SceneManager.LoadScene("BattleOne");
                }
                else if (state == Playerstate.Level0)
                {
                    SceneManager.LoadScene("MenuSystem");
                }
                else
                {
                    error = true;
                    error5.SetActive(true);
                    StartCoroutine(errorMessage(error5));
                }
            } else if (Input.GetKeyDown(KeyCode.Space))
            {
                error = true;
                error2.SetActive(true);
                StartCoroutine(errorMessage(error2));
            } else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)){
                error = true;
                error1.SetActive(true);
                StartCoroutine(errorMessage(error1));
            }
        }
    }

    private void move(bool left)
    {
        if (left)
        {
            StartCoroutine(movePlayer(this.transform, new Vector3(this.transform.position.x - 55f, this.transform.position.y, this.transform.position.z), 3.0f));
        }
        else
        {
            StartCoroutine(movePlayer(this.transform, new Vector3(this.transform.position.x + 55f, this.transform.position.y, this.transform.position.z), 3.0f));
        }
    }

    IEnumerator errorMessage(GameObject message)
    {
        yield return new WaitForSeconds(3.0f);
        message.SetActive(false);
        error = false;
    }

    IEnumerator movePlayer(Transform fromPosition, Vector3 toPosition, float duration)
    {
        if (moving)
        {
            yield break;
        }
        moving = true;

        float counter = 0;

        Vector3 startPos = fromPosition.position;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            fromPosition.position = Vector3.Lerp(startPos, toPosition, counter / duration);
            yield return null;
        }

        moving = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "level0")
        {
            left = false;
            right = true;
            state = Playerstate.Level0;
        }

        if (collision.gameObject.name == "level1")
        {
            left = true;
            if (level2)
            {
                right = true;
            }
            else
            {
                right = false;
            }
            state = Playerstate.Level1;
        }

        if (collision.gameObject.name == "level2")
        {
            if (level3)
            {
                right = true;
            }
            else
            {
                right = false;
            }
            left = true;
            state = Playerstate.Level2;
        }

        if (collision.gameObject.name == "shop")
        {
            if (level2)
            {
                right = true;
            }
            else
            {
                right = false;
            }
            left = true;
            state = Playerstate.Shop;
        }

        if(collision.gameObject.name == "level3")
        {
            right = false;
            left = true;
            state = Playerstate.Level3;
        }
    }
}
