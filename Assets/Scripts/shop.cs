//simple shop - user can spend stars here to buy powerups which affect the gameplay during the 'battle' with the AI

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class shop : MonoBehaviour
{
    [SerializeField]
    Text starText;

    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    int starscollected = 0;
    int starsspent = 0;

    int health = 0;
    int bullets = 0;
    int enemy = 0;

    bool addedhealth = false;
    bool addedbullets = false;
    bool addedenemy = false;

    string previousFile = "previouslevel.txt";
    string shopFile = "shop.txt";

    void Awake()
    {
        using (TextReader file = File.OpenText(shopFile))
        {
            string text = null;
            int i = 0;
            while ((text = file.ReadLine()) != null)
            {
                if(i == 0)
                {
                    starscollected = int.Parse(text);
                }
                if (i == 1)
                {
                    starsspent = int.Parse(text);
                }
                else if (i == 2)
                {
                    health = int.Parse(text);
                }
                else if (i == 3)
                {
                    bullets = int.Parse(text);
                }
                else if (i == 4)
                {
                    enemy = int.Parse(text);
                }
                i++;
            }
        }
        starText.text = "" + starscollected;
    }

    void Start()
    {
        Menu();
        button4.onClick.AddListener(BackToHomepage);
    }

    void Menu()
    {
        if(starscollected > 0)
        {
            if (health == 0 && !addedhealth)
            {
                button1.onClick.AddListener(Health);
                addedhealth = true;
            }
            else if (health == 1)
            {
                button1.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
                button1.onClick.RemoveListener(Health);
            }

            if (bullets == 0 && !addedbullets)
            {
                button2.onClick.AddListener(Bullets);
                addedbullets = true;
            }
            else if (bullets == 1)
            {
                button2.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
                button2.onClick.RemoveListener(Bullets);
            }

            if (enemy == 0 && !addedenemy)
            {
                button3.onClick.AddListener(Enemy);
                addedenemy = true;
            }
            else if (enemy == 1)
            {
                button3.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
                button3.onClick.RemoveListener(Enemy);
            }
        }
        else
        {
            button1.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            button1.onClick.RemoveListener(Health);
            button2.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            button2.onClick.RemoveListener(Bullets);
            button3.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            button3.onClick.RemoveListener(Enemy);
        }
    }

    private void Health()
    {
        health = 1;
        starscollected -= 1;
        starsspent++;
        lineChanger("" + starscollected, shopFile, 1);
        lineChanger("" + starsspent, shopFile, 2);
        lineChanger("" + health, shopFile, 3);
        starText.text = "" + starscollected;
        Menu();
    }

    private void Bullets()
    {
        bullets = 1;
        starscollected -= 1;
        starsspent++;
        lineChanger("" + starscollected, shopFile, 1);
        lineChanger("" + starsspent, shopFile, 2);
        lineChanger("" + bullets, shopFile, 4);
        starText.text = "" + starscollected;
        Menu();
    }

    private void Enemy()
    {
        enemy = 1;
        starscollected -= 1;
        starsspent++;
        lineChanger("" + starscollected, shopFile, 1);
        lineChanger("" + starsspent, shopFile, 2);
        lineChanger("" + enemy, shopFile, 5);
        starText.text = "" + starscollected;
        Menu();
    }

    static public void lineChanger(string newStars, string fileName, int line_to_edit)
    {
        string[] arrLine = File.ReadAllLines(fileName);
        arrLine[line_to_edit - 1] = newStars;
        File.WriteAllLines(fileName, arrLine);
    }

    void BackToHomepage()
    {
        File.Create(previousFile).Close();
        using (StreamWriter sw = File.AppendText(previousFile))
        {
            sw.Write("shop");
        }
        SceneManager.LoadScene("Overworld");
    }
}
