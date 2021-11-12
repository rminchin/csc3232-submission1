//simple menu system using UI text fields and buttons

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class MenuController : MonoBehaviour
{

    [SerializeField]
    GameObject menuUI;

    [SerializeField]
    GameObject instructionsUI;

    [SerializeField]
    GameObject startingUI;

    [SerializeField]
    GameObject gameplayUI;

    [SerializeField]
    GameObject powerupsUI;

    public Button button1;
    public Button button2;

    public Button button3;
    public Button button4;
    public Button button5;
    public Button button6;

    public Button button7;
    public Button button8;

    public Button button9;
    public Button button10;

    public Button button11;
    public Button button12;

    string previousFile = "previouslevel.txt";

    void Start()
    {
        StartMainMenu();
    }

    private void StartMainMenu()
    {
        menuUI.SetActive(true);
        instructionsUI.SetActive(false);
        startingUI.SetActive(false);
        gameplayUI.SetActive(false);
        powerupsUI.SetActive(false);

        button1.onClick.AddListener(delegate { InputChoice("StartGame"); });
        button2.onClick.AddListener(delegate { InputChoice("Instructions"); });
    }

    private void InputChoice(string choice)
    {
        if (choice == "StartGame")
        {
            StartGame();
        }
        else if (choice == "Instructions")
        {
            StartInstructions();
        } else if (choice == "StartingGame")
        {
            StartGameInstructions();
        } else if (choice == "Gameplay")
        {
            StartGameplayInstructions();
        } else if (choice == "Powerups")
        {
            StartPowerupsInstructions();
        }
    }

    private void StartGame()
    {
        File.Create(previousFile).Close();
        using (StreamWriter sw = File.AppendText(previousFile))
        {
            sw.Write("menu");
        }
        SceneManager.LoadScene("Overworld");
    }

    private void StartInstructions()
    {
        menuUI.SetActive(false);
        instructionsUI.SetActive(true);
        gameplayUI.SetActive(false);
        powerupsUI.SetActive(false);
        startingUI.SetActive(false);

        button3.onClick.AddListener(delegate { InputChoice("StartingGame"); });
        button4.onClick.AddListener(delegate { InputChoice("Gameplay"); });
        button5.onClick.AddListener(delegate { InputChoice("Powerups"); });
        button6.onClick.AddListener(StartMainMenu);
    }

    private void StartGameInstructions()
    {
        menuUI.SetActive(false);
        instructionsUI.SetActive(false);
        gameplayUI.SetActive(false);
        powerupsUI.SetActive(false);
        startingUI.SetActive(true);

        button7.onClick.AddListener(StartInstructions);
        button8.onClick.AddListener(StartGameplayInstructions);
    }

    private void StartGameplayInstructions()
    {
        menuUI.SetActive(false);
        instructionsUI.SetActive(false);
        gameplayUI.SetActive(true);
        powerupsUI.SetActive(false);
        startingUI.SetActive(false);

        button9.onClick.AddListener(StartGameInstructions);
        button10.onClick.AddListener(StartPowerupsInstructions);
    }

    private void StartPowerupsInstructions()
    {
        menuUI.SetActive(false);
        instructionsUI.SetActive(false);
        gameplayUI.SetActive(false);
        powerupsUI.SetActive(true);
        startingUI.SetActive(false);

        button11.onClick.AddListener(StartGameplayInstructions);
        button12.onClick.AddListener(StartInstructions);
    }
}
