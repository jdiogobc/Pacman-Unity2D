using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private GameManager gameManager;
    public AudioSource musicAndSounds;
    public AudioSource chompsAudioSource; //I still don't have too much experience with audiosources so i will make two to pay safe. One just for the chomps.


    public AudioClip death;
    public AudioClip chomp;
    public AudioClip spooked;
    public AudioClip themeSong;
    public AudioClip beginning;

    float timeWhereWeLeft;

    private GhostMove.MovementMode blinky;
    private GhostMove.MovementMode inky;
    private GhostMove.MovementMode pinky;
    private GhostMove.MovementMode clyde;

    // Start is called before the first frame update
    void Start()
    {
        chompsAudioSource.clip = chomp;
        AssignGhosts();
        gameManager = FindObjectOfType<GameManager>();
        ResetTheme();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (musicAndSounds.clip == spooked)
        {
            if (!IsAnyoneScared())
                ReturnToTheme();
        }
    }

    /// <summary>
    /// plays the main theme from the beginning
    /// </summary>
    public void ResetTheme()
    {
        musicAndSounds.clip = themeSong;
        musicAndSounds.time = 0;
        musicAndSounds.Play();
    }

    /// <summary>
    /// plays the main theme from the point it stoped
    /// </summary>
    public void ReturnToTheme()
    {
        musicAndSounds.clip = themeSong;
        musicAndSounds.time = timeWhereWeLeft;
        musicAndSounds.Play();
    }

    /// <summary>
    /// pauses whatever is playing
    /// </summary>
    public void PauseTheme()
    {
        timeWhereWeLeft = musicAndSounds.time;
        musicAndSounds.Pause();
    }

    /// <summary>
    /// Plays the tension music. credits to Toby Fox
    /// </summary>
    public void SpookMusic()
    {
        if (musicAndSounds.clip = themeSong)
            timeWhereWeLeft = musicAndSounds.time;
        musicAndSounds.clip = spooked;
        musicAndSounds.time = 0;
        musicAndSounds.Play();
    }

    /// <summary>
    /// Plays the chomp sound
    /// </summary>
    public void Chomp()
    {
        if (!chompsAudioSource.isPlaying)
            chompsAudioSource.Play();
    }

    /// <summary>
    /// gets info asbout the ghosts so that I know when they are spooked or not and stop the music accordingly
    /// </summary>
    private void AssignGhosts()
    {
        blinky = GameObject.Find("Blinky").GetComponent<GhostMove>().movementMode;
        inky = GameObject.Find("Inky").GetComponent<GhostMove>().movementMode;
        pinky = GameObject.Find("Pinky").GetComponent<GhostMove>().movementMode;
        clyde = GameObject.Find("Clyde").GetComponent<GhostMove>().movementMode;
    }

    /// <summary>
    /// returns true if anyone is still scared
    /// </summary>
    /// <returns>if anyone is scared this will be true</returns>
    private bool IsAnyoneScared()
    {
        AssignGhosts();
        return blinky == GhostMove.MovementMode.Spooked ||
            inky == GhostMove.MovementMode.Spooked ||
            pinky == GhostMove.MovementMode.Spooked ||
            clyde == GhostMove.MovementMode.Spooked;

    }

    /// <summary>
    /// plays the pacman death sfx
    /// </summary>
    public void KillPacmanSFX()
    {

        musicAndSounds.clip = death;
        musicAndSounds.time = 0;
        musicAndSounds.Play();
    }

}
