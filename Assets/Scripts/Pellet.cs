using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pellet : MonoBehaviour
{
    GameManager gameManager;
    UIManager uIManager;
    SoundManager soundManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        uIManager = FindObjectOfType<UIManager>();
        soundManager = FindObjectOfType<SoundManager>();
    }

/// <summary>
/// if pacman collides with a pellet if is consumed, makes a sound and adds score. If no pellet is left it triggers the loading of a new lelvel
/// </summary>
/// <param name="other"></param>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Pacman")
        {
            GameManager.score += 10;
            soundManager.Chomp();
            uIManager.UpdateScore(GameManager.score);
            gameManager.pellets.Remove(gameObject);
            Destroy(gameObject);

            if (gameManager.pellets.Count == 0)
            {
                gameManager.LoadLevel();
            }
        }
    }
}
