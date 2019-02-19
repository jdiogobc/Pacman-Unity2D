using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Canvas pauseCanvas;
    public Canvas readyCanvas;
    public Canvas gameOverCanvas;
    public GameObject LivesHolder;

    public Text levelText;
    public Text scoreText;
    public Text survivalTimeText;
    public Text highScoreText;
    public Text highSurvivalTimeText;
    public Text GOScoreText;
    public Text GOSurvivalTime;
    public Text GOHighScoreText;

    private SoundManager soundManager;

    private void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();
        UpdateHighScoresAndTimes();

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            Pause();
            soundManager.PauseTheme();
        }
    }

    /// <summary>
    /// Shows or hides the Ready Canvas depending on the input
    /// </summary>
    /// <param name="_bool"></param>
    public void ReadyCanvasToggle(bool _bool)
    {
        readyCanvas.enabled = _bool;
    }

    /// <summary>
    /// Disables any canvas that may be in front of the game
    /// </summary>
    public void ReturnToGame()
    {
        Time.timeScale = 1;
        pauseCanvas.enabled = false;
        gameOverCanvas.enabled = false;
        readyCanvas.enabled = false;
    }

    /// <summary>
    /// Pauses the game, stops time
    /// </summary>
    public void Pause()
    {
        Time.timeScale = 0;
        pauseCanvas.enabled = true;
    }

    /// <summary>
    /// Takes care of the GameOver UI-wise 
    /// </summary>
    public void GameOver()
    {
        gameOverCanvas.enabled = true;        
        GOSurvivalTime.text = survivalTimeText.text;
        GOScoreText.text = scoreText.text;

        if (GameManager.score >= GameManager.highScore)
            GOHighScoreText.text = "Congrats! you are the new HighScore!";
        else
            GOHighScoreText.text = ("Almost there, only " +
                (GameManager.highScore- GameManager.score) + 
                "to reach the HighScore!");
    }

    public void UpdateScore(int _score)
    {
        scoreText.text = ("Score: \n" + _score.ToString());
    }

    public void UpdateTime(int _time)
    {
        survivalTimeText.text = ("Survival Time: \n" + _time.ToString());
    }

    public void UpdateLevel(int _level)
    {
        levelText.text = ("Level: \n" +_level.ToString());
    }

    /// <summary>
    /// updates how many little pacmen are in the "pacman holdes"
    /// </summary>
    /// <param name="_lives"></param>
    public void UpdateLives(int _lives)
    {
        Image[] livesArray = LivesHolder.GetComponentsInChildren<Image>();

        for (int i = 0; i < 3; i++)
        {
            if (i < _lives)
            {
                livesArray[i].enabled = true;
            }
            else
                livesArray[i].enabled = false; ;

        }
    }

    /// <summary>
    /// Quits the game
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Updates HighScores and Highsurvival times in the UI
    /// </summary>
    public void UpdateHighScoresAndTimes()
    {
        highSurvivalTimeText.text = ("High Survival Time: \n" + GameManager.highSurvivalTime.ToString());
        highScoreText.text = ("HighScore: \n" + GameManager.highScore.ToString());
    }
}
