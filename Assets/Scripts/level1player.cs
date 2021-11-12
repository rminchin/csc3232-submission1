using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor;

public class level1player : MonoBehaviour
{
    private Rigidbody2D b;
    private CircleCollider2D circle;
    private GameObject blocker;
    private GameObject blocker2;
    private GameObject blocker3;
    private GameObject[] starsTotal;
    private GameObject hiddenPlatform;
    private GameObject hiddenStar;
    private GameObject star2;
    private GameObject star3;
    private GameObject[] hiddenTriangles;
    private GameObject hiddenPlatform2;
    private GameObject hiddenPowerup1;
    private GameObject hiddenPowerup2;
    private GameObject platform;
    public GameObject wallnew;
    private GameObject wallold;
    public GameObject wallnew1;
    private GameObject wallold1;

    public GameObject wall1;
    public GameObject wall2;

    public GameObject wholewall1;
    public GameObject wholewall2;

    private Vector2 respawnpoint;
    private bool bounced;
    private bool collected;
    private bool previouslycollected;
    private bool collected2;
    private bool collected3;
    private bool respawned;

    [SerializeField]
    string starsFile = "stars-level1.txt";

    string shopFile = "shop.txt";
    string previousFile = "previouslevel.txt";

    public GameObject finishedUI;

    private enum PlayerState
    {
        Normal,
        Jumping,
        SpedUp,
        GravityUp,
        GravityUpFloating,
        GravityDown,
        GravityDownFloating,
        DoubleJumping,
        MovingPlatform,
        Bouncy,
        Slippy,
        Finished
    }

    [SerializeField]
    float maxSpeed = 2f;

    [SerializeField]
    Text starText;

    private int stars = 0;
    private int starsatspawn = 0;

    private PlayerState state = PlayerState.Normal;
    private PlayerState previousState = PlayerState.Normal;

    private void Awake()
    {
        respawnpoint = this.transform.position;
        Physics2D.gravity = new Vector2(0f, -9.8f);
        b = transform.GetComponent<Rigidbody2D>();
        circle = transform.GetComponent<CircleCollider2D>();
        blocker = GameObject.Find("Blocker1");
        blocker.SetActive(false);
        blocker2 = GameObject.Find("Blocker2");
        blocker2.SetActive(false);
        blocker3 = GameObject.Find("Blocker3");
        blocker3.SetActive(false);
        starsTotal = GameObject.FindGameObjectsWithTag("Star");
        hiddenPlatform = GameObject.Find("Platform4");
        hiddenPlatform.GetComponent<Renderer>().enabled = false;
        hiddenStar = GameObject.Find("Star1");
        star2 = GameObject.Find("Star2");
        star3 = GameObject.Find("Star3");
        hiddenStar.GetComponent<Renderer>().enabled = false;
        hiddenTriangles = GameObject.FindGameObjectsWithTag("HiddenObstacle");
        foreach (GameObject o in hiddenTriangles)
        {
            o.GetComponent<Renderer>().enabled = false; //some game objects are hidden from view until passing through a trigger, when they become visible
        }
        hiddenPlatform2 = GameObject.Find("Platform5");
        hiddenPowerup1 = GameObject.Find("GravityChangeUp1");
        hiddenPowerup2 = GameObject.Find("GravityChangeDown1");
        hiddenPlatform2.GetComponent<Renderer>().enabled = false;
        hiddenPowerup1.GetComponent<Renderer>().enabled = false;
        hiddenPowerup2.GetComponent<Renderer>().enabled = false;
        wallnew.SetActive(false);
        wallold = GameObject.Find("WallOld");
        wallnew1.SetActive(false);
        wallold1 = GameObject.Find("WallOld1");
        bounced = false;
        collected = false;
        collected2 = false;
        collected3 = false;
        using (TextReader file = File.OpenText(starsFile))
        {
            string text = null;
            string[] splits;
            int i = 0;
            while ((text = file.ReadLine()) != null) //finds out which stars, if any, user collected in this level on previous attempts and hides already collected stars
            {
                if(i == 0)
                {
                    splits = text.Split(' ');
                    if ((splits[0]) == "1")
                    {
                        previouslycollected = true;
                        collected = true;
                        hiddenStar.SetActive(false);
                        stars += 1;
                        starsatspawn += 1;
                    }
                    if ((splits[1]) == "1")
                    {
                        collected2 = true;
                        star2.gameObject.SetActive(false);
                        stars += 1;
                        starsatspawn += 1;
                    }
                    if ((splits[2]) == "1")
                    {
                        collected3 = true;
                        star3.gameObject.SetActive(false);
                        stars += 1;
                        starsatspawn += 1;
                    }
                }
                break;
            }
        }
        starText.text = stars + "/" + starsTotal.Length;
        respawned = false;
    }
    
    private void Update()
    {
        if(state == PlayerState.Finished && Input.GetKeyDown(KeyCode.Return))
        {
            File.Create(previousFile).Close();
            using (StreamWriter sw = File.AppendText(previousFile))
            {
                sw.Write("level1");
            }
            SceneManager.LoadScene("Overworld"); //sends player back to appropriate icon in overworld scene
        }

        if(Physics2D.gravity == new Vector2(0f, -9.8f) && state == PlayerState.GravityUp)
        {
            state = PlayerState.GravityDown;
        }

        if (state == PlayerState.GravityDown)
        {
            state = PlayerState.Normal;
        }

        if(state != PlayerState.SpedUp && state != PlayerState.GravityUpFloating && state != PlayerState.GravityDownFloating && state != PlayerState.DoubleJumping && state != PlayerState.MovingPlatform && state != PlayerState.Bouncy && state != PlayerState.Finished)
        {
            float jump = 5f;
            if (state == PlayerState.Normal && state != PlayerState.Jumping && Input.GetKeyDown(KeyCode.Space))
            {
                b.velocity = Vector2.up * jump;
                previousState = state;
                state = PlayerState.Jumping; //code for 'normal' jumping with 'normal' gravity
            }

            else if (state != PlayerState.Jumping && state == PlayerState.GravityUp && Input.GetKeyDown(KeyCode.Space))
            {
                b.velocity = Vector2.down * jump;
                previousState = state;
                state = PlayerState.Jumping; //code for inversed jumping with inversed gravity
            }
        }

        moving();

    }

    private void moving()
    {
        if (state != PlayerState.SpedUp && state != PlayerState.GravityUpFloating && state != PlayerState.GravityDownFloating && state != PlayerState.DoubleJumping && state != PlayerState.MovingPlatform && state != PlayerState.Finished)
        {
            if(state != PlayerState.Slippy)
            {
                maxSpeed = 2f;
                if (Input.GetKey(KeyCode.A))
                {
                    b.velocity = new Vector2(-maxSpeed, b.velocity.y);
                }
                else
                {
                    if (Input.GetKey(KeyCode.D))
                    {
                        b.velocity = new Vector2(+maxSpeed, b.velocity.y);
                    }
                    else
                    {
                        b.velocity = new Vector2(0, b.velocity.y);
                    }
                }
            }
            else
            { //hard-coded change of friction, much more gradual change in velocity upon pressing buttons down
                maxSpeed = 3f;
                if (Input.GetKey(KeyCode.A))
                {
                    if (b.velocity.x > -maxSpeed)
                    {
                        b.velocity = new Vector2(b.velocity.x - 0.005f, b.velocity.y);
                    }
                }

                if(Input.GetKey(KeyCode.D))
                {
                    if (b.velocity.x < maxSpeed)
                    {
                        b.velocity = new Vector2(b.velocity.x + 0.005f, b.velocity.y);
                    }
                }
            }
        }
    }

    bool startedMovingPlatform = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Finish"))
        {
            state = PlayerState.Finished;
            b.velocity = new Vector2(0f,0f);
            Physics2D.gravity = new Vector2(0f, 0f);
            b.angularVelocity = 0f;
            finishedUI.SetActive(true);

            File.Create(starsFile).Close();
            using (StreamWriter sw = File.AppendText(starsFile))
            {
                foreach (GameObject star in starsTotal)
                {
                    if (star.gameObject.activeSelf)
                    {
                        sw.Write("0 ");
                    }
                    else
                    {
                        sw.Write("1 ");
                    }

                }
                sw.Write("\n");
                sw.Write("" + stars); //writes which stars were collected/not collected and then the total amount collected
            }

            if (collected)
            {
                starsatspawn -= 1;
            }


            int starsthistime = stars - starsatspawn;
            int starsinfile = 0;
            using (TextReader file = File.OpenText(shopFile))
            {
                string text = null;
                while ((text = file.ReadLine()) != null)
                {
                    starsinfile = int.Parse(text); //writes how many stars are now available to spend into the shop file
                    break;
                }
            }
            int starstowrite = starsinfile + starsthistime;
            shop.lineChanger("" + starstowrite, shopFile, 1);
        }

        if (collision.gameObject.CompareTag("BouncyPlatform"))
        {
            if (state == PlayerState.Jumping)
            {
                state = previousState; //maximum bounciness
            }
        }

        if (collision.gameObject.CompareTag("Platform")){
            if(state == PlayerState.GravityUpFloating)
            {
                state = PlayerState.GravityUp;
            } else if(state == PlayerState.GravityDownFloating)
            {
                state = PlayerState.GravityDown;
            }
            else if(state != PlayerState.MovingPlatform)
            {
                state = previousState;
            }
            maxSpeed = 2f;
        }

        if (collision.gameObject.CompareTag("SlipperyPlatform"))
        {
            state = PlayerState.Slippy; //no friction
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Respawn(); //user respawns if they hit an obstacle
        }

        if (collision.gameObject.CompareTag("MovingPlatformDown"))
        {
            platform = collision.gameObject;
            if(state == PlayerState.Jumping || state == PlayerState.GravityUpFloating)
            {
                state = previousState;
            }

            GameObject platform2;
            GameObject mock1;
            GameObject mock2;
            platform2 = GameObject.Find("MovingPlatform2");
            mock1 = GameObject.Find("MockPlatform1");
            mock2 = GameObject.Find("MockPlatform2");

            if (!startedMovingPlatform)
            {
                startedMovingPlatform = true;
                state = PlayerState.MovingPlatform;
                StartCoroutine(movePlatform(platform.transform, mock1.transform.position, 3.0f));
                StartCoroutine(movePlatform2(platform2.transform, mock2.transform.position, 3.0f)); //certain platforms move around during gameplay
            }
        }

        if (collision.gameObject.CompareTag("MovingPlatformUp"))
        {
            platform = collision.gameObject;
            GameObject mock3;
            mock3 = GameObject.Find("MockPlatform3");
            GameObject mock4;
            mock4 = GameObject.Find("MockPlatform4");
            
            if(state == PlayerState.Jumping || state == PlayerState.GravityDownFloating)
            {
                state = previousState;
            }

            GameObject platform2;
            platform2 = GameObject.Find("MovingPlatform1");
            if (startedMovingPlatform)
            {
                state = PlayerState.MovingPlatform;
                StartCoroutine(movePlatform(platform.transform, new Vector3(mock3.transform.position.x, mock3.transform.position.y, 0.0f), 3.0f));
                StartCoroutine(movePlatform2(platform2.transform, new Vector3(mock4.transform.position.x, mock4.transform.position.y, 0.0f), 3.0f)); //certain platforms move around during gameplay
            }
        }

        if (collision.gameObject.CompareTag("MockPlatform"))
        {
            if (state == PlayerState.MovingPlatform)
            {
                state = PlayerState.Normal;
                startedMovingPlatform = false;
            }
        }
    }

    bool isMovingPlatformDown = false;
    bool isMovingPlatformDown2 = false;

    IEnumerator movePlatform(Transform fromPosition, Vector3 toPosition, float duration)
    {
        if (isMovingPlatformDown)
        {
            yield break;
        }
        isMovingPlatformDown = true;

        float counter = 0;
        b.velocity = new Vector2(0f, b.velocity.y);

        Vector3 startPos = fromPosition.position;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            fromPosition.position = Vector3.Lerp(startPos, toPosition, counter / duration);
            yield return null;
        }

        isMovingPlatformDown = false; //code to move platforms using gameplay
    }

    IEnumerator movePlatform2(Transform fromPosition, Vector3 toPosition, float duration)
    {
        if (isMovingPlatformDown2)
        {
            yield break;
        }
        isMovingPlatformDown2 = true;

        float counter = 0;

        Vector3 startPos = fromPosition.position;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            fromPosition.position = Vector3.Lerp(startPos, toPosition, counter / duration);
            yield return null;
        }

        isMovingPlatformDown2 = false; //code so two different platforms can move simultaneously
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("checkpoint"))
        {
            if (collision.gameObject.name == "checkpoint1" && !respawned)
            {
                respawnpoint = new Vector2(29.51f, -0.14f);
                if (!hiddenStar.activeSelf && !previouslycollected)
                {
                    collected = true;
                    starsatspawn++;
                }
                respawned = true; //there is a checkpoint flag around halfway through - user will respawn here once they pass it
            }
        }

        if (collision.gameObject.CompareTag("SpeedUp"))
        {
            maxSpeed = 5f;
            state = PlayerState.SpedUp;
            Physics2D.gravity = Vector2.zero;
            b.velocity = new Vector2(maxSpeed, 0f);
            StartCoroutine(speedPowerupEnd(1f)); //powerup to temporarily increase speed
        }

        if (collision.gameObject.CompareTag("GravityUp"))
        {
            Physics2D.gravity = Vector2.zero;
            maxSpeed = 10f;
            b.velocity = new Vector2(0f, maxSpeed);
            state = PlayerState.GravityUpFloating; //inverses gravity
        }

        if (collision.gameObject.CompareTag("GravityDown"))
        {
            maxSpeed = -10f;
            b.velocity = new Vector2(0f, maxSpeed);
            state = PlayerState.GravityDownFloating;
            Physics2D.gravity = Vector2.zero; //gravity back to normal
        }

        if (collision.gameObject.CompareTag("DoubleJump"))
        {
            float jump = 7f;
            maxSpeed = 7f;
            b.velocity = new Vector2(maxSpeed, jump);
            state = PlayerState.DoubleJumping; //longer jump powerup
        }

        if (collision.gameObject.CompareTag("MockPlatform"))
        {
            if (state == PlayerState.MovingPlatform)
            {
                state = PlayerState.Normal;
                startedMovingPlatform = false;
            }
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            if (state == PlayerState.SpedUp)
            {
                collision.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                collision.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                if (collision.gameObject.name == "Wall")
                {
                    wallold.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                    wallold.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    wallnew.SetActive(true);
                    StartCoroutine(DeleteWall(wallnew));
                }
                else
                {
                    wallold1.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                    wallold1.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    wallnew1.SetActive(true);
                    StartCoroutine(DeleteWall(wallnew1)); //colliding with 'walls' causes them to break into smaller pieces
                }
            }
        }

        if (collision.gameObject.CompareTag("Trigger"))
        {
            if (collision.gameObject.name == "Trigger1")
            {
                StartCoroutine(triggerActive(0.2f, blocker));
            }
            else if (collision.gameObject.name == "Trigger2")
            {
                hiddenPlatform.GetComponent<Renderer>().enabled = true;
                if (!collected)
                {
                    hiddenStar.GetComponent<Renderer>().enabled = true;
                }
                foreach (GameObject o in hiddenTriangles)
                {
                    o.GetComponent<Renderer>().enabled = true;
                }
                hiddenPlatform2.GetComponent<Renderer>().enabled = true;
                hiddenPowerup1.GetComponent<Renderer>().enabled = true;
                hiddenPowerup2.GetComponent<Renderer>().enabled = true;
            } else if (collision.gameObject.name == "GravityTrigger")
            {
                if (state == PlayerState.GravityDownFloating)
                {
                    Physics2D.gravity = new Vector2(0f, -9.8f);
                }
                else if (state == PlayerState.GravityUpFloating)
                {
                    Physics2D.gravity = new Vector2(0f, 9.8f);
                }
                maxSpeed = 2f;
            } else if (collision.gameObject.name == "MovingTrigger")
            {
                state = PlayerState.Normal;
            } else if (collision.gameObject.name == "BounceTrigger")
            {
                if (state != PlayerState.Bouncy && !bounced)
                {
                    PhysicsMaterial2D newMaterial = new PhysicsMaterial2D("Bouncy");
                    newMaterial.bounciness = 1f;
                    b.sharedMaterial = newMaterial;
                    previousState = PlayerState.Normal;
                    state = PlayerState.Bouncy;
                    blocker2.SetActive(true);
                    bounced = true; //code to give player maximum bounciness using physicsmaterial2D
                }
            } else if (collision.gameObject.name == "BounceTrigger1")
            {
                PhysicsMaterial2D newMaterial = new PhysicsMaterial2D("NotBouncy");
                newMaterial.bounciness = 0f;
                b.sharedMaterial = newMaterial;
                StartCoroutine(triggerActive(0.5f, blocker3));
                state = PlayerState.Normal; //code to put player back to normal bounciness using physicsmaterial2D
            } else if (collision.gameObject.name == "SlipperyTrigger")
            {
                if (state != PlayerState.Slippy)
                {
                    previousState = PlayerState.Normal;
                    state = PlayerState.Slippy;
                }
                else
                {
                    state = PlayerState.Normal;
                }
            }
        }

        if (collision.gameObject.CompareTag("Star"))
        {
            stars++;
            starText.text = stars + "/" + starsTotal.Length;
            collision.gameObject.SetActive(false);
        }

        if ((collision.gameObject.CompareTag("Respawn")) || (collision.gameObject.CompareTag("Obstacle")) || (collision.gameObject.CompareTag("HiddenObstacle")))
        {
            bool cont = false;
            if (collision.gameObject.name == "Respawn4")
            {
                if (state != PlayerState.GravityDownFloating && state != PlayerState.GravityUpFloating)
                {
                    cont = true;
                }
                else
                {
                    collision.gameObject.GetComponent<Renderer>().enabled = false;
                }
            }
            else
            {
                cont = true;
            }

            if (cont)
            {
                Respawn();
            }
        }
    }

    private IEnumerator DeleteWall(GameObject wall)
    {
        int i = 0;
        GameObject[] allChildren = new GameObject[wall.transform.childCount];

        foreach (Transform child in wall.transform)
        {
            allChildren[i] = child.gameObject;
            i += 1;
        }

        yield return new WaitForSeconds(1.5f);

        foreach (GameObject child in allChildren)
        {
            child.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            child.gameObject.SetActive(false);
        }
    }

    private IEnumerator speedPowerupEnd(float wait)
    {
        yield return new WaitForSeconds(wait);
        maxSpeed = 2f;
        Physics2D.gravity = new Vector2(0f, -9.8f);
        state = PlayerState.Normal;
    }

    private IEnumerator triggerActive(float wait, GameObject block)
    {
        yield return new WaitForSeconds(wait);
        block.SetActive(true);
    }

    private void Respawn()
    {
        state = PlayerState.Normal;
        Physics2D.gravity = new Vector2(0f, -9.8f);
        b.velocity = new Vector2(b.velocity.x, 0f);
        this.transform.position = respawnpoint;

        blocker.SetActive(false);
        blocker2.SetActive(false);
        blocker3.SetActive(false);
        stars = starsatspawn;
        starText.text = stars + "/" + starsTotal.Length;
        hiddenPlatform.GetComponent<Renderer>().enabled = false;
        if (!collected)
        {
            hiddenStar.SetActive(true);
        }
        if (!collected2)
        {
            star2.SetActive(true);
        }
        if (!collected3)
        {
            star3.SetActive(true);
        }
        hiddenStar.GetComponent<Renderer>().enabled = false;
        foreach (GameObject o in hiddenTriangles)
        {
            o.GetComponent<Renderer>().enabled = false;
        }
        hiddenPlatform2.GetComponent<Renderer>().enabled = false;
        hiddenPowerup1.GetComponent<Renderer>().enabled = false;
        hiddenPowerup2.GetComponent<Renderer>().enabled = false;
        bounced = false; //sets all variables/hidden parts of the level back to how they were at 'awake' and moves player back to respawn point
    }
}
