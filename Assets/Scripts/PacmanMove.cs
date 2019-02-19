using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacmanMove : MonoBehaviour
{
    public float speed = 0.4f;
    [HideInInspector]
    public Vector2 destination = Vector2.zero;

    private Vector2 direction = Vector2.zero;
    private Vector2 nextDirection = Vector2.zero;
    private GameManager gameManager;

    private Rigidbody2D rigidbody2D;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        rigidbody2D = GetComponent<Rigidbody2D>();
        gameManager = FindObjectOfType<GameManager>();
        destination = transform.position;
    }

    void FixedUpdate()
    {
        ReadInputAndMove();
        Animate();
    }



    /// <summary>
    /// this function read user input and stores it for future. When the user is indeed able to turn he will turn (may be at that instant) ATTENTION this may crash if the intial positions is not a cool int number!
    /// </summary>
    void ReadInputAndMove()
    {
        // move closer to destination
        Vector2 p = Vector2.MoveTowards(transform.position, destination, speed);
        rigidbody2D.MovePosition(p);

        // get the next direction from keyboard
        if (Input.GetAxis("Horizontal") > 0) nextDirection = Vector2.right;
        if (Input.GetAxis("Horizontal") < 0) nextDirection = Vector2.left;
        if (Input.GetAxis("Vertical") > 0) nextDirection = Vector2.up;
        if (Input.GetAxis("Vertical") < 0) nextDirection = Vector2.down;

        // if pacman is in the center of a tile
        if (Vector2.Distance(destination, transform.position) < 0.00001f)
        {
            if (Valid(nextDirection)) //if our input (next direction) is valid
            {
                destination = (Vector2)transform.position + nextDirection;
                direction = nextDirection;
            }
            else   // if next direction is not valid
            {
                if (Valid(direction))  // and the prev. direction is valid
                    destination = (Vector2)transform.position + direction;   // continue on that direction

                // otherwise, do nothing
            }
        }
    }

    /// <summary>
    /// takes care of the animation using the players direction
    /// </summary>
    void Animate()
    {
        Vector2 dir = destination - (Vector2)transform.position;
        animator.SetFloat("DirX", dir.x);
        animator.SetFloat("DirY", dir.y);
    }

    /// <summary>
    /// Raycasts to check walls in this direction
    /// </summary>
    /// <param name="_direction"> direction to raycast</param>
    /// <returns> Is this direction a valid one? </returns>
    bool Valid(Vector2 _direction)
    {

        // cast line from pacman to 'next to pacman'
        Vector2 pos = transform.position;
        _direction += new Vector2(_direction.x * .45f, _direction.y * .45f);

        RaycastHit2D hit = Physics2D.Linecast(pos, pos + _direction);
        if (hit)
        {
            return hit.collider.tag != "Wall";
        }
        else
        {
            return true;
        }

    }

    /// <summary>
    /// If pacman colides with a ghost it may kill it or be killed
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ghost")
        {
            // If the ghost is spooked kill it
            if (collision.GetComponent<GhostMove>().movementMode == GhostMove.MovementMode.Spooked)
            {
                collision.GetComponent<GhostMove>().KillGhost();
                GameManager.score += 100; //TODO make it 100 200 500 1000 for streaks;
            }
            else if (collision.GetComponent<GhostMove>().movementMode != GhostMove.MovementMode.Dead)
            {
                animator.SetFloat("DirX", 0);
                animator.SetFloat("DirY", 0);
                animator.SetBool("Dead", true);
                gameManager.PacmanDeath();
            }
        }
    }


    /// <summary>
    /// for outside acess to direction
    /// </summary>
    /// <returns></returns>
    public Vector2 getDirection()
    {
        return direction;
    }
}
