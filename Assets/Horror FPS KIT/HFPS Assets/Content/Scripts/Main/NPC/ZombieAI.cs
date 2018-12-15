/*
 * ZombieAI.cs - written by ThunderWire Games
 * Version 1.1
*/

using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using ThunderWire.Helper.Random;

[RequireComponent(typeof(NPCHealth))]
public class ZombieAI : MonoBehaviour
{
    [HideInInspector] public NPCHealth npcHealth;
    private NavMeshAgent navMeshA;
    private GameObject player;
    private AnimatorStateInfo stateInfo;

    [Header("Main Setup")]
    public Animator zombieAnimator;
    public LayerMask SearchMask;
    public LayerMask PlayerMask;
    public LayerMask AttractionLayer;
    public PatrolPoint[] patrolPoints;

    [Header("Sensors")]
    public AttackTrigger AttackCollider;
    public WaypointGroup Waypoints;

    [Header("Sensor Settings")]
    [Range(0, 179)]
    public float zombieFOVAngle;
    public float chaseDistance;
    public float closeDistance;
    public float veryCloseDistance;
    public float seeDistance;
    public float patrolPointDetect;
    public float zombieHeight;

    [Header("AI Settings")]
    [Range(0, 2)]
    public int intelligence;
    public float walkSpeed;
    public float runSpeed;
    public float lookSpeed;
    public float chaseStopDistance;
    public int patrolTime;
    public int patrolPointTime;
    public bool randomPatrol;
    public bool waypointPatrol;

    [Header("AI Damage Settings")]
    [Tooltip("Attack Damage given to Player (From-To)")]
    public Vector2 AttackDamage;

    [Space(7)]
    public bool enableGizmos;

    private float defaultStopDistance;

    private int patrolRandomTime;
    private Vector3 lastSeenPos;
    private Vector3 patrolPointPos;
    private Vector3 zombiePos;
    private float playerDistance;
    private int path;

    [HideInInspector]
    public bool isAttracted;
    private bool playerChased;
    private bool increase;
    private bool patrol;
    private bool patrolPending;
    private bool goLastPos;
    private bool isDead = false;
    private bool playerDead = false;
    private bool playerRunning = false;
    private bool turn = false;

    private Vector3 oldAgentPos;

    private int num = -1;

    private float angle;
    private GameObject attractionCube;

    private List<int> Numbers = new List<int>();

    private void Awake()
    {
        navMeshA = GetComponent<NavMeshAgent>();
        player = Camera.main.transform.root.gameObject;
        patrolPoints = FindObjectsOfType<PatrolPoint>();

        if (GetComponent<NPCHealth>())
        {
            npcHealth = GetComponent<NPCHealth>();
        }
    }

    void Start()
    {
        defaultStopDistance = navMeshA.stoppingDistance;

        navMeshA.autoBraking = false;
        navMeshA.isStopped = false;
        zombiePos = transform.position;
        zombiePos.y = zombieHeight;
    }

    void Update()
    {
        stateInfo = zombieAnimator.GetCurrentAnimatorStateInfo(0);
        playerDead = player.GetComponent<HealthManager>().isDead;
        AttackCollider.Enabled = !playerDead;

        if (enableGizmos)
        {
            Debug.Log("Player Distance: " + playerDistance);
        }

        if (!npcHealth) return;

        Vector3 targetDir = transform.position - player.transform.position;
        angle = Vector3.SignedAngle(targetDir, transform.forward, Vector3.up);

        if (player && !isDead)
        {
            playerDistance = Vector3.Distance(transform.position, player.transform.position);
            playerRunning = player.GetComponent<PlayerController>().run;

            if (AttackCollider.PlayerInTrigger)
            {
                StopAllCoroutines();
                patrol = false;
                navMeshA.isStopped = true;
                SetAnimatorState("isAttacking", true, true);
            }
            else
            {
                if (!stateInfo.IsName("Attack"))
                {
                    if (intelligence > 0 && npcHealth.damageTaken && !isAttracted)
                    {
                        isAttracted = true;
                        if (!turn && intelligence > 1) { AttractZombie(); }
                    }

                    if (SearchForPlayer())
                    {
                        ResetAgent();
                        StopAllCoroutines();

                        navMeshA.isStopped = false;
                        navMeshA.stoppingDistance = chaseStopDistance;
                        navMeshA.speed = runSpeed;

                        playerChased = true;
                        if (intelligence > 0) { isAttracted = true; }
                        patrol = false;
                        SetAnimatorState("isRunning", true, true);
                        zombieAnimator.SetBool("isTurning", false);
                        lastSeenPos = player.transform.position;
                        navMeshA.SetDestination(lastSeenPos);
                        patrolPointPos = Vector3.zero;
                        goLastPos = false;
                        Destroy(attractionCube);
                    }
                    else
                    {
                        navMeshA.stoppingDistance = defaultStopDistance;

                        if (playerChased)
                        {
                            navMeshA.isStopped = false;
                            if (!goLastPos)
                            {
                                navMeshA.SetDestination(lastSeenPos);
                                goLastPos = true;
                            }

                            if (patrolPoints.Length > 0)
                            {
                                foreach (var i in patrolPoints)
                                {
                                    if (i.InTrigger)
                                    {
                                        patrolPointPos = i.transform.position;
                                    }
                                }
                            }

                            if (pathComplete())
                            {
                                if (intelligence > 0)
                                {
                                    if (patrolPointPos != Vector3.zero )
                                    {
                                        float distance = Vector3.Distance(lastSeenPos, patrolPointPos);
                                        Debug.Log("Current distance: " + distance + " Detect at: " + patrolPointDetect);
                                        if (distance <= patrolPointDetect)
                                        {
                                            Debug.Log("Setting Destination to Patrol Point");
                                            SetAnimatorState("isWalking", true, true);
                                            navMeshA.speed = walkSpeed;
                                            navMeshA.SetDestination(patrolPointPos);
                                            StartCoroutine(GoToPatrolPoint());

                                            foreach (var i in patrolPoints)
                                            {
                                                if (i.zombieInTrigger)
                                                {
                                                    i.zombieInTrigger = false;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    StartCoroutine(Patrol(patrolTime));
                                }

                                playerChased = false;
                                isAttracted = false;
                                npcHealth.damageTaken = false;
                            }
                        }
                        else
                        {
                            if (!patrol && !isAttracted && !turn)
                            {
                                navMeshA.isStopped = false;
                                playerChased = false;
                                WaypointSequence();
                            }
                        }
                    }
                }
                else
                {
                    SetAnimatorState("isRunning", true, true);
                }
            }
        }
    }

    void SetAnimatorState(string parameter, bool state, bool disableOthers)
    {
        if (disableOthers)
        {
            zombieAnimator.SetBool("Patrol", false);
            zombieAnimator.SetBool("isRunning", false);
            zombieAnimator.SetBool("isWalking", false);
            zombieAnimator.SetBool("isAttacking", false);
        }

        if (state)
        {
            zombieAnimator.SetBool(parameter, true);
        }
        else
        {
            zombieAnimator.SetBool(parameter, false);
        }
    }

    void WaypointSequence()
    {
        if (pathComplete())
        {
            if (waypointPatrol)
            {
                if (!patrolPending)
                {
                    SetAnimatorState("isWalking", true, true);
                    navMeshA.speed = walkSpeed;
                    navMeshA.SetDestination(Waypoints.Waypoints[path].position);
                    increase = false;
                }
                else
                {
                    patrolRandomTime = Random.Range(1, patrolTime);
                    StartCoroutine(Patrol(patrolRandomTime));
                }
                patrolPending = true;
            }
            else
            {
                SetAnimatorState("isWalking", true, true);
                navMeshA.speed = walkSpeed;
                navMeshA.SetDestination(Waypoints.Waypoints[path].position);
                increase = false;
            }

            NextWaypoint();
        }
    }

    private void NextWaypoint()
    {
        if (randomPatrol)
        {
            if (Waypoints.Waypoints.Count > 1 && !increase)
            {
                path = GetWaypointRandom();
                increase = true;
            }
            else
            {
                path = path == Waypoints.Waypoints.Count - 1 ? 0 : path + 1;
            }
        }
        else
        {
            if (path < Waypoints.Waypoints.Count && !increase)
            {
                path++;
                if (path > (Waypoints.Waypoints.Count - 1)) path = 0;
                increase = true;
            }
        }
    }

    private int GetWaypointRandom()
    {
        if (Numbers.Count == 0)
        {
            Numbers = Randomizer.RandomList(0, Waypoints.Waypoints.Count, Waypoints.Waypoints.Count);
        }

        if (num != Waypoints.Waypoints.Count - 1)
        {
            num++;
            return Numbers[num];
        }
        else
        {
            Numbers.Clear();
            num = 0;
            return 0;
        }
    }

    IEnumerator Patrol(int Time)
    {
        SetAnimatorState("Patrol", true, true);
        patrol = true;
        yield return new WaitForSeconds(Time);
        patrol = false;
        patrolPending = false;
        yield return 0;
    }

    IEnumerator GoToPatrolPoint()
    {
        patrol = true;
        yield return new WaitUntil(() => pathComplete());
        SetAnimatorState("Patrol", true, true);
        yield return new WaitForSeconds(patrolPointTime);
        patrol = false;
        patrolPointPos = Vector3.zero;
        yield return 0;
    }

    public void AttackPlayer()
    {
        float randomDamage = Random.Range(AttackDamage.x, AttackDamage.y);
        if (AttackCollider.PlayerInTrigger)
            player.GetComponent<HealthManager>().ApplyDamage(randomDamage);
    }

    public void StateMachine(bool enabled)
    {
        if (!enabled)
        {
            isDead = true;
            zombieAnimator.enabled = false;
            navMeshA.isStopped = true;
        }
        else
        {
            StopAllCoroutines();
            isDead = false;
            zombieAnimator.enabled = true;
            navMeshA.isStopped = false;
        }
    }

    private bool pathComplete()
    {
        if (Vector3.Distance(navMeshA.destination, navMeshA.transform.position) <= navMeshA.stoppingDistance)
        {
            if (!navMeshA.hasPath || navMeshA.velocity.sqrMagnitude <= 0.05f)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check if Zombie is detected the Player
    /// </summary>
    private bool SearchForPlayer()
    {
        RaycastHit hit;

        if (playerDead) return false;

        if (Physics.Linecast(transform.position, player.transform.position, out hit, SearchMask))
        {
            if (!playerChased)
            {
                if (hit.collider.gameObject == player && playerDistance <= seeDistance)
                {
                    if (!isAttracted)
                    {
                        if (IsLookingAtPlayer())
                        {
                            StopAllCoroutines();
                            turn = false;
                            return true;
                        }
                        else
                        {
                            if (playerRunning && playerDistance <= closeDistance)
                            {
                                if (!turn && intelligence > 1) { AttractZombie(); }
                            }
                        }
                    }
                    else
                    {
                        if (IsLookingAtPlayer())
                        {
                            StopAllCoroutines();
                            turn = false;
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (playerDistance <= chaseDistance)
                {
                    if (IsLookingAtPlayer())
                    {
                        return true;
                    }
                    else
                    {
                        if (hit.collider.gameObject == player)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (IsLookingAtPlayer() && hit.collider.gameObject == player)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void AttractZombie()
    {
        oldAgentPos = transform.position;
        navMeshA.speed = lookSpeed;
        navMeshA.ResetPath();
        navMeshA.updatePosition = false;
        StopAllCoroutines();
        StartCoroutine(AttractedTurn());
    }

    IEnumerator AttractedTurn()
    {
        turn = true;

        zombieAnimator.SetBool("isTurning", true);
        SetAnimatorState("isWalking", false, true);

        if (angle > 0)
        {
            zombieAnimator.SetTrigger("TurnRight");
            yield return null;
        }
        else
        {
            zombieAnimator.SetTrigger("TurnLeft");
            yield return null;
        }

        if (!attractionCube)
        {
            attractionCube = Instantiate(Resources.Load<GameObject>("AttractionCube"));
            attractionCube.GetComponent<Collider>().isTrigger = true;
            Vector3 pos = player.transform.position;
            pos.y = transform.position.y;
            attractionCube.transform.position = pos;
            yield return null;
        }
       
        navMeshA.SetDestination(attractionCube.transform.position);
        yield return new WaitUntil(() => isLookingAtAttractDir());

        StartCoroutine(Patrol(patrolPointTime));
        yield return new WaitUntil(() => !patrol);

        SetAnimatorState("isWalking", true, true);
        zombieAnimator.SetBool("isTurning", false);
        Destroy(attractionCube);

        ResetAgent();
        npcHealth.damageTaken = false;
        isAttracted = false;
        turn = false;

        yield return 0;
    }

    bool isLookingAtAttractDir()
    {
        RaycastHit hit;

        float rayDistance = Vector3.Distance(transform.position, attractionCube.transform.position) + 1;

        if (enableGizmos)
        {
            Vector3 fwd = transform.TransformDirection(Vector3.forward);
            Debug.DrawRay(transform.position, fwd * rayDistance, Color.green);
        }

        if (Physics.Raycast(transform.position, transform.forward, out hit, rayDistance, AttractionLayer))
        {
            return true;
        }

        return false;
    }

    private bool IsLookingAtPlayer()
    {
        float checkAngle = Mathf.Min(zombieFOVAngle, 359.9999f) / 2;

        float dot = Vector3.Dot(transform.forward, (player.transform.position - transform.position).normalized);

        float viewAngle = (1 - dot) * 90;

        if (viewAngle <= checkAngle)
            return true;
        else
            return false;
    }

    private void ResetAgent()
    {
        if (oldAgentPos != Vector3.zero)
        {
            navMeshA.Warp(oldAgentPos);
            navMeshA.updatePosition = true;
            navMeshA.ResetPath();
            oldAgentPos = Vector3.zero;
        }
    }

    public Dictionary<string, object> GetZombieData()
    {
        Dictionary<string, object> NPCData = new Dictionary<string, object>
        {
            { "position", transform.localPosition },
            { "patrolPointPos", patrolPointPos },
            { "lastSeenPos", lastSeenPos },
            { "rotation", transform.localEulerAngles.y },
            { "isDead", isDead },
            { "npcHealth", npcHealth.Health },
            { "isAttracted", isAttracted },
            { "path", path },
            { "patrolPending", patrolPending },
            { "patrol", patrol },
            { "turn", turn }
        };
        return NPCData;
    }

    public void SetZombieData(Newtonsoft.Json.Linq.JToken token)
    {
        bool isDead = (bool)token["isDead"];
        bool patrol = (bool)token["patrol"];
        bool patrolPending = (bool)token["patrolPending"];
        bool turn = (bool)token["turn"];

        if (isDead)
        {
            Destroy(gameObject);
        }
        else
        {
            transform.localPosition = token["position"].ToObject<Vector3>();
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, (float)token["rotation"], transform.eulerAngles.z);

            GetComponent<NPCHealth>().Health = (int)token["npcHealth"];

            isAttracted = (bool)token["isAttracted"];
            path = (int)token["path"];

            if (patrolPending)
            {
                GoZombiePatrolPoint(
                    token["patrolPointPos"].ToObject<Vector3>(),
                    token["lastSeenPos"].ToObject<Vector3>());
            }
            else
            {
                if (patrol && !turn)
                {
                    GoZombiePatrol();
                }
                else if (turn)
                {
                    AttractZombie();
                }
            }
        }
    }

    public void GoZombiePatrol()
    {
        StopAllCoroutines();
        navMeshA.ResetPath();
        patrolPending = true;
        SetAnimatorState("isWalking", true, true);
        navMeshA.speed = walkSpeed;
        navMeshA.SetDestination(Waypoints.Waypoints[path].position);
        increase = false;
    }

    public void GoZombiePatrolPoint(Vector3 patrolPoint, Vector3 lastPos)
    {
        StopAllCoroutines();
        navMeshA.ResetPath();
        patrolPointPos = patrolPoint;
        lastSeenPos = lastPos;
        goLastPos = true;
        playerChased = true;
    }

    void OnDrawGizmosSelected()
    {
        float rayRange = 10.0f;
        float halfFOV = zombieFOVAngle / 2.0f;

        if (!enableGizmos) return;

        Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);

        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, leftRayDirection * rayRange);
        Gizmos.DrawRay(transform.position, rightRayDirection * rayRange);
        Gizmos.DrawRay(zombiePos, transform.forward * rayRange);
    }
}
