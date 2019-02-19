using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform otherTeleportPod;
    public PacmanMove pacmanController;

    void Start()
    {
        pacmanController = FindObjectOfType<PacmanMove>();    
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        pacmanController.destination = otherTeleportPod.position;
        collision.transform.position = otherTeleportPod.position;
    }

}
