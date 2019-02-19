using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private Vector2 pacmanInitialPosition;
    private Vector2 blinkyInitialPosition;
    private Vector2 boxCenterPosition = new Vector2(13.5f, 16f); //#Hardcoded

    private UIManager uIManager;
    private SoundManager soundManager;
    private PacmanMove pacmanMove;

    private GameObject pacman;
    private GameObject blinky;
    private GameObject inky;
    private GameObject pinky;
    private GameObject clyde;
    [HideInInspector]
    public List<GameObject> pellets;
    [HideInInspector]
    public int initialpellets;

    public float spookTime = 10; // How much time the ghosts are spooked by the energizer
    public float deadTime = 5; // How much time the ghosts remain in the box after being killed
    private bool gameisRunning; // bool mostly to manage timers, it is true when the game is running


    static public int Level = 1;
    static public int lives = 3;
    static public int score;
    static public float surivivalTime;
    static public int highScore;
    static public int highSurvivalTime;

    private static GameManager _instance;
    public static GameManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameManager>();
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        UpdateHighScoresAndTimes();

        highScore = PlayerPrefs.GetInt("HighScore");
        highSurvivalTime = PlayerPrefs.GetInt("HighSurvivalTime");



        uIManager = FindObjectOfType<UIManager>();
        soundManager = FindObjectOfType<SoundManager>();

        uIManager.UpdateLives(lives);
        uIManager.UpdateLevel(Level);
        uIManager.UpdateHighScoresAndTimes();

        pacmanMove = FindObjectOfType<PacmanMove>();
        AssignGhosts();
        pellets.AddRange(GameObject.FindGameObjectsWithTag("Pellets"));
        initialpellets = pellets.Count;


        EveryoneStop();
        StartCoroutine(ResetEveryone(2));
        //StartCoroutine(StartGameCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        if (gameisRunning)
        {
        surivivalTime += Time.deltaTime;
        uIManager.UpdateTime((int)surivivalTime);
        }
    }


    /// <summary>
    /// Search for the ghosts in the scene to know who is who
    /// </summary>
    void AssignGhosts()
    {
        // find and assign ghosts
        blinky = GameObject.Find("Blinky");
        inky = GameObject.Find("Inky");
        pinky = GameObject.Find("Pinky");
        clyde = GameObject.Find("Clyde");
        pacman = GameObject.Find("Pacman");

        blinkyInitialPosition = blinky.transform.position;
        pacmanInitialPosition = pacman.transform.position;

        //Sanity Check
        if (clyde == null || pinky == null || inky == null || blinky == null) Debug.Log("One of ghosts are NULL");
        if (pacman == null) Debug.Log("Pacman is NULL");
    }

    /// <summary>
    /// Spooks all the ghosts
    /// </summary>
    public void SpookGhosts()
    {
        blinky.GetComponent<GhostMove>().Spook();
        inky.GetComponent<GhostMove>().Spook();
        pinky.GetComponent<GhostMove>().Spook();
        clyde.GetComponent<GhostMove>().Spook();
    }

    /// <summary>
    /// Orders everyone to stop what they are doing
    /// </summary>
    public void EveryoneStop()
    {
        gameisRunning = false;
        pacmanMove.enabled = false;
        blinky.GetComponent<GhostMove>().StopMoving();
        pinky.GetComponent<GhostMove>().StopMoving();
        inky.GetComponent<GhostMove>().StopMoving();
        clyde.GetComponent<GhostMove>().StopMoving();
    }

    /// <summary>
    /// Resets everyone to the original positions an waits a small delay to restart the movements and take the "Ready" Canvas
    /// </summary>
    /// <param name="_waitTime"> time to wait before giving the controls back</param>
    /// <returns></returns>
    public IEnumerator ResetEveryone(float _waitTime)
    {
        pacman.transform.position = pacmanInitialPosition;
        blinky.transform.position = blinkyInitialPosition;
        pinky.transform.position = boxCenterPosition;
        inky.transform.position = boxCenterPosition;
        clyde.transform.position = boxCenterPosition;
        
        pacman.GetComponent<Animator>().SetBool("Dead", false);

        soundManager.ResetTheme();

        uIManager.ReadyCanvasToggle(true);
        yield return new WaitForSeconds(_waitTime);
        uIManager.ReadyCanvasToggle(false);

        gameisRunning = true;
        pacmanMove.enabled = true;
        pacmanMove.destination = pacmanInitialPosition;

        blinky.GetComponent<GhostMove>().Start();
        pinky.GetComponent<GhostMove>().Start();
        inky.GetComponent<GhostMove>().Start();
        clyde.GetComponent<GhostMove>().Start();
    }

    /// <summary>
    /// kills pacman, if there were no lifes left- gameover, else it takes a life and starts the pacmandeathcoroutine
    /// </summary>
    public void PacmanDeath()
    {
        soundManager.PauseTheme();
        EveryoneStop();
        if (lives == 0)
        {
            soundManager.KillPacmanSFX();
            GameOver();
        }
        else
        {
            lives -= 1;
            uIManager.UpdateLives(lives);
            StartCoroutine(PacmanDeathCoroutine());
        }
    }

    /// <summary>
    /// it waits 1 second and then plays an animation and waits 2 more before reseting everything
    /// </summary>
    /// <returns></returns>
    public IEnumerator PacmanDeathCoroutine()
    {
        yield return new WaitForSeconds(1); // little pause
        soundManager.KillPacmanSFX();
        yield return new WaitForSeconds(2); // time for dead animation to finish without reseting positions

        StartCoroutine(ResetEveryone(3)); // 3 seconds to get ready
        yield return null;

    }

    /// <summary>
    /// Load the next Level
    /// </summary>
    public void LoadLevel()
    {
        Level++;
        SceneManager.LoadScene("Game");
    }

    /// <summary>
    /// Finishes the game
    /// </summary>
    public void GameOver()
    {
        UpdateHighScoresAndTimes();
        uIManager.UpdateHighScoresAndTimes();        
        uIManager.GameOver();
        EveryoneStop();
    }

    /// <summary>
    /// Restarts the game
    /// </summary>
    public void RestartGame()
    {
        UpdateHighScoresAndTimes();

        EveryoneStop();

        SceneManager.LoadScene("Game");
        Level = 1;
        lives = 3;
        surivivalTime = 0;
        score = 0;
    }

    /// <summary>
    /// Updates HighScores
    /// </summary>
    public void UpdateHighScoresAndTimes()
    {
        if (surivivalTime > highSurvivalTime)
        {
            PlayerPrefs.SetInt("HighSurvivalTime", (int)surivivalTime);
        }
        if (score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
        }
    }
}
