using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostMove : MonoBehaviour
{
    private AStarAlgorithm astar;
    private GameManager gameManager;
    private PacmanMove pacmanController;
    private Animator animator;

    private Transform pacmanTransform;
    private Transform blinkyTransform; // because inky uses it and we dont want to get component every time we update a target

    public Node[] scatterNodes; // Nodes where this ghost scatters to, have to drag them from the scene
    public Node startNode;

    private List<Node> pathToObjective; // path of nodes
    private Node currentNode; // Node where ghost is or was
    private Node targetNode; // Next node to go
    private Vector2 target; // Where to go

    public enum GhostName
    {
        Blinky,
        Pinky,
        Inky,
        Clyde
    }
    public GhostName ghostName;

    public enum MovementMode
    {
        Chase,
        Scatter,
        Spooked,
        Dead,
        Waiting
    }
    public MovementMode movementMode;

    private float chaseTimer;
    private float scatterTimer;
    private float spookedTimer;
    private float deadTimer;

    private float chaseTime; // may be later changed to the gameManager
    private float scatterTime; // may be later changed to the gameManager
    //float spookedTime = 5; // is on game manager
    //float deadTime =2 // is on game manager
    private int chaseCycle;

    public float speed;

    private void Awake()
    {
        astar = GetComponent<AStarAlgorithm>();
        pacmanController = GameObject.FindGameObjectWithTag("Pacman").GetComponent<PacmanMove>();
        pacmanTransform = GameObject.FindGameObjectWithTag("Pacman").transform;
        gameManager = FindObjectOfType<GameManager>();
        movementMode = MovementMode.Waiting;
        blinkyTransform = GameObject.Find("Blinky").transform;
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// This is called not only on the start of the match but also when the game is reset
    /// </summary>
    public void Start()
    {
        setTimerTo(0);
        currentNode = startNode;
        movementMode = MovementMode.Waiting;
        StartCoroutine(BeginningCoroutine());
        speed = UpdateGhostSpeed();
    }

    void Update()
    {
        Animate();
        TimersManager();

        if (gameManager.pellets.Count <= 20) // this checks if we are getting to the agressive blinky stage
            speed = UpdateGhostSpeed();
    }

    /// <summary>
    /// Waits until the ghost can indeed move and sets everything up so that he can start moving
    /// </summary>
    /// <returns></returns>
    IEnumerator BeginningCoroutine()
    {
        yield return new WaitUntil(() => CanIStart());
        movementMode = MovementMode.Scatter;
        target = UpdadeTarget();
        pathToObjective = astar.FindPath(currentNode, GetClosesteNode(target));
        targetNode = pathToObjective[pathToObjective.Count - 1];
        StartCoroutine(MovetoNextNod());
    }

    /// <summary>
    /// This function checks if the ghost can leave the box. Inky and Clyde only leave if a certain amount of pellets were eaten. TODO vary pellet number with level
    /// </summary>
    /// <returns></returns>
    bool CanIStart()
    {
        switch (ghostName)
        {
            case (GhostName.Blinky):
                return true;
            case (GhostName.Pinky):
                return true;
            case (GhostName.Inky):
                return gameManager.pellets.Count <= gameManager.initialpellets - 30;
            case (GhostName.Clyde):
                return gameManager.pellets.Count <= gameManager.initialpellets - 60;
            default:
                Debug.Log("Default case");
                return true;
        }
    }


    /// <summary>
    /// Manages the timers that change the ghost from mode to mode.
    /// </summary> 
    void TimersManager()
    {
        if (movementMode == MovementMode.Scatter)
        {
            scatterTimer += Time.deltaTime;
            if (scatterTimer >= scatterTime)
            {
                movementMode = MovementMode.Chase;
            }
        }

        else if (movementMode == MovementMode.Chase)
        {
            chaseTimer += Time.deltaTime;
            if (chaseTimer >= chaseTime)
            {
                chaseCycle += 1;
                UpdateChaseScatterTimes();
                movementMode = MovementMode.Scatter;
            }
        }
        else if (movementMode == MovementMode.Spooked)
        {
            spookedTimer += Time.deltaTime;
            if (spookedTimer > gameManager.spookTime)
            {
                spookedTimer = 0;
                if (scatterTimer >= scatterTime)
                {
                    movementMode = MovementMode.Chase;
                }
                else
                {
                    movementMode = MovementMode.Scatter;
                }
            }

        }
    }

    /// <summary>
    /// it chanses how much time the ghost should spend in each mode, it quite hardcoded, but I followed the standart times //TODO maybe change to game manager
    /// </summary>
    void UpdateChaseScatterTimes()
    {
        // TODO make it level dependent
        chaseTimer = 0;
        scatterTimer = 0;
        switch (chaseCycle)
        {
            case (1):
                scatterTime = 7;
                chaseTime = 20;
                break;
            case (2):
                scatterTime = 7;
                chaseTime = 20;
                break;
            case (3):
                scatterTime = 5;
                chaseTime = 20;
                break;
            case (4):
                scatterTime = 5;
                chaseTime = Mathf.Infinity;
                break;
        }
    }


    /// <summary>
    /// This function updates the location of the target depending no Ghost type and Movement Type - Some of the movement is also coded in the Spook and Kill function
    /// </summary>
    /// <returns></returns>
    Vector2 UpdadeTarget()
    {
        // I inspired my behaviours here: http://gameinternals.com/post/2072558330/understanding-pac-man-ghost-behavior

        if (movementMode == MovementMode.Chase)
        {
            switch (ghostName)
            {
                case GhostName.Blinky: // he follows pacman
                    target = (Vector2)pacmanTransform.position;
                    break;

                case GhostName.Pinky: // he tried to ambush pacman by going 4 tiles in front of him
                    target = (Vector2)pacmanTransform.position + 4 * pacmanController.getDirection();

                    break;

                case GhostName.Inky:
                    Vector2 tempTarget = (Vector2)pacmanController.transform.position + 2 * pacmanController.getDirection();
                    target = (Vector2)blinkyTransform.position + (tempTarget - (Vector2)blinkyTransform.position) * -2;
                    break;

                case GhostName.Clyde:
                    if (Vector2.Distance(transform.position, (Vector2)pacmanTransform.position) > 8)
                        target = (Vector2)pacmanTransform.position;
                    else
                        target = scatterNodes[0].transform.position;
                    break;
            }
        }

        else if (movementMode == MovementMode.Scatter)
        {
            for (int i = 0; i < scatterNodes.Length; i++)
            {
                if (targetNode == scatterNodes[i]) // is it in any of the scatter nodes?
                {
                    target = scatterNodes[(i + 1) % scatterNodes.Length].transform.position; //if so, go to the next one and break the cycle
                    break;
                }
                if (i == scatterNodes.Length - 1) // if we are in the end and the cycle didn't break go to the first point
                {
                    target = scatterNodes[0].transform.position;
                }
            }
        }

        else if (movementMode == MovementMode.Spooked) //they cant start freightened because they have no target node yet
        {
            target = targetNode.neighbours[Random.Range(0, targetNode.numberOfNeighbours)].transform.position; //goes to a random node neighbour to the current node
        }

        return target;
    }


    /// <summary>
    /// Moves the ghost to the next nod and when it arrives to the next nod updates his target and starts a new coroutine.
    /// </summary>
    /// <returns></returns>
    IEnumerator MovetoNextNod()
    {
        while (true)
        {
            if (movementMode != MovementMode.Waiting)
            {
                while (Vector3.Distance(transform.position, targetNode.transform.position) > 0.1)
                {
                    Vector2 p = Vector2.MoveTowards(transform.position,
                                        targetNode.transform.position,
                                        speed);
                    GetComponent<Rigidbody2D>().MovePosition(p);
                    yield return null;
                }
                pathToObjective.Clear(); // clearn previous path just in case
                currentNode = targetNode; // updates current node
                target = UpdadeTarget(); //updates target (vector2)
                pathToObjective = astar.FindPath(currentNode, GetClosesteNode(target)); // finds path to the node closeste to target
                targetNode = pathToObjective[pathToObjective.Count - 1]; // target node is the last one from the path (the closest to the ghost
            }
            //StartCoroutine(MovetoNextNod()); // restarts this
            yield return null;
        }
    }


    /// <summary>
    /// Takes care of animation
    /// </summary>
    void Animate()
    {
        if (movementMode != MovementMode.Waiting)
        {
            Vector2 dir = targetNode.transform.position - transform.position;
            animator.SetFloat("DirX", dir.x);
            animator.SetFloat("DirY", dir.y);
        }


        if (movementMode == MovementMode.Spooked)
        {
            animator.SetBool("Spooked", true);
        }
        else
        {
            animator.SetBool("Spooked", false);
        }
        if (spookedTimer > gameManager.spookTime * 0.7f)
        {
            animator.SetBool("Unspooking", true);
        }
        else
        {
            animator.SetBool("Unspooking", false);
        }

        if (movementMode == MovementMode.Dead)
        {
            animator.SetBool("Dead", true);
        }
        else
        {
            animator.SetBool("Dead", false);

        }


    }


    /// <summary>
    /// This function will give us the closest nod to the targeted position. If the ghost is on that nod it will return the second closest node
    /// </summary>
    /// <param name="_targetPosition"> position where to look for a node</param>
    /// <returns></returns>
    Node GetClosesteNode(Vector2 _targetPosition)
    {
        // The minimum distance we are looking at lateron
        float minDistance = float.MaxValue;
        float secondMinDistance = float.MaxValue;

        GameObject closestNode = null;
        GameObject secondClosestNode = null;


        // Get the collisions
        Collider2D[] hitColliders = FindObjectsOfType<Collider2D>(); //Physics2D.OverlapCircleAll(_targetPosition,Mathf);

        for (int i = 0; i < hitColliders.Length; ++i)
        {
            // Check, if the collision object has the correct tag
            if (hitColliders[i].tag.Equals("Nodes"))
            {
                // Get the position of the collider we are looking at
                Vector2 possiblePosition = hitColliders[i].transform.position;

                // Get the distance between the point and the current nod
                float currDistance = Vector2.Distance(_targetPosition, possiblePosition);

                // If the distance is smaller than the one before...
                if (currDistance < minDistance)
                {
                    // Assign gameobject
                    secondClosestNode = closestNode;
                    closestNode = hitColliders[i].gameObject;

                    // Set our compare-value to that one
                    secondMinDistance = minDistance;
                    minDistance = currDistance;
                }
                else if (currDistance < secondMinDistance) // This helps me return the second minimum for when the ghost is already on the target point

                {
                    secondMinDistance = currDistance;
                    secondClosestNode = hitColliders[i].gameObject;
                }

            }
        }

        // this will return the closes node unless the ghost is already on it.
        if (Vector3.Distance(transform.position, closestNode.transform.position) <= 0.45)
        {
            return secondClosestNode.GetComponent<Node>();
        }
        else
        {
            return closestNode.GetComponent<Node>();
        }

    }


    /// <summary>
    /// if he is alive, spookes the ghost - makes a u turn and changes behaviour
    /// </summary>
    public void Spook()
    {
        if (movementMode != MovementMode.Dead && movementMode != MovementMode.Waiting)
        {

            spookedTimer = 0;
            movementMode = MovementMode.Spooked;

            StopAllCoroutines();
            pathToObjective.Clear();
            targetNode = currentNode; // U turn
            StartCoroutine(MovetoNextNod());

        }
    }


    /// <summary>
    /// Kills the ghost, starts the dead coroutine
    /// </summary>
    public void KillGhost()
    {
        movementMode = MovementMode.Dead;
        StopAllCoroutines();
        StartCoroutine(DeadCoroutine());
    }


    /// <summary>
    /// forces ghosts to stop moving
    /// </summary>
    public void StopMoving()
    {
        StopAllCoroutines();
        movementMode = MovementMode.Waiting;
        animator.SetBool("Dead", false);
        animator.SetFloat("DirX", 0);
        animator.SetFloat("DirY", 0);
    }


    /// <summary>
    /// This coroutine takes the dead ghosts to the house and makes them wait a little bit until reviving
    /// it lack style because it repeats a little bit of code, but solves some bugs
    /// </summary>
    /// <returns></returns>
    IEnumerator DeadCoroutine()
    {
        while (Vector3.Distance(transform.position, startNode.transform.position) > 0.1)
        {

            while (Vector3.Distance(transform.position, targetNode.transform.position) > 0.1)
            {
                Vector2 p = Vector2.MoveTowards(transform.position,
                                    targetNode.transform.position,
                                    speed * 3); // dead ghosts move kinda fast #hardcoded
                GetComponent<Rigidbody2D>().MovePosition(p);
                yield return null;
            }
            pathToObjective.Clear();
            currentNode = targetNode;
            pathToObjective = astar.FindPath(currentNode, startNode);
            if (pathToObjective.Count != 0)
                targetNode = pathToObjective[pathToObjective.Count - 1];
            yield return null;
        }

        yield return new WaitForSeconds(gameManager.deadTime);

        if (scatterTimer >= scatterTime)
        {
            movementMode = MovementMode.Chase;
        }
        else
        {
            movementMode = MovementMode.Scatter;
        }

        StartCoroutine(MovetoNextNod());
        yield return null;
    }


    /// <summary>
    /// this is meant to reset the timers of the ghosts, usefull for pacman deaths or changing levels
    /// </summary>
    /// <param name="_time"></param>
    void setTimerTo(float _time)
    {
        scatterTimer = 0;
        chaseTimer = 0;
        spookedTimer = 0;
        chaseCycle = 0;
    }

    /// <summary>
    /// this updates the correct ghost speed for eveyr phase of the game
    /// </summary>
    /// <returns></returns>
    float UpdateGhostSpeed()
    {
        float speedToReturn;

        if (GameManager.Level == 1)
            speedToReturn = pacmanController.speed * .75f;
        else if (GameManager.Level < 5)
            speedToReturn =  pacmanController.speed * .85f;
        else
            speedToReturn = pacmanController.speed * .95f;

        if (ghostName == GhostName.Blinky)
        {
            if (gameManager.pellets.Count <= 20)
                speedToReturn += pacmanController.speed * .05f;
            if (gameManager.pellets.Count <= 10)
                speedToReturn += pacmanController.speed * .05f;
        }

        return speedToReturn;

    }

}
