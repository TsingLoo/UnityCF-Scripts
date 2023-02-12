using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class BotController : BasePawnController
{
    #region AI
    [Header("AI")]
    public bool _stop;
    public float attackRange = 1;
    public NavMeshAgent navMeshAgent;               //  Nav mesh agent component
    public float startWaitTime = 4;                 //  Wait time of every action
    public float timeToRotate = 2;                  //  Wait time when the enemy detect near the player without seeing
    public float speedWalk = 6;                     //  Walking speed, speed in the nav mesh agent
    public float speedRun = 9;                      //  Running speed

    public float viewAngle = 90;                    //  Angle of the enemy view
    public float viewRadius = 15;                   //  Radius of the enemy view
    public LayerMask playerMask;                    //  To detect the player with the raycast
    public LayerMask obstacleMask;                  //  To detect the obstacules with the raycast
    public float meshResolution = 1.0f;             //  How many rays will cast per degree
    public int edgeIterations = 4;                  //  Number of iterations to get a better performance of the mesh filter when the raycast hit an obstacule
    public float edgeDistance = 0.5f;               //  Max distance to calcule the a minumun and a maximum raycast when hits something

    // todo set random points // SearchWalkPoint
    public WayPoint[] wayPoints { get; set; }
    public Transform[] wayPointTransforms { get; set; }
    int m_CurrentWayPointIndex;                     //  Current waypoint where the enemy is going to

    //  Last position of the player when was near the enemy
    Vector3 m_playerLastPosition = Vector3.zero;      
    //  Last position of the player when the player is seen by the enemy
    Vector3 m_PlayerPosition;
    Transform _playerTransform;
    PlayerController _player;

    float m_WaitTime;                               //  Variable of the wait time that makes the delay
    float m_TimeToRotate;                           //  Variable of the wait time to rotate when the player is near that makes the delay
    bool m_playerInSight;                           //  If the player is in range of vision, state of chasing
    bool m_PlayerNear;                              //  If the player is near, state of hearing
    //  If the enemy is patrol, state of patroling
    bool m_IsPatrol;                                
    bool m_playerInAttackRange;                            //  if the enemy has caught the player
    #endregion

    protected override void Awake()
    {
        base.Awake();

        //this.Setlayer(EditorLayer.Enemy);
        weapon1PLayer = EditorLayer.PlayerNoSee;
        weapon3PLayer = EditorLayer.Enemy;

        _player = GameObject.FindObjectOfType<PlayerController>();
        _playerTransform = _player.transform;

        _stop = false;
        if (!wayPointTransforms.HasValue())
        {
            wayPoints = GameObject.FindObjectsOfType<WayPoint>();
            Debug.Assert(wayPoints.HasValue()
                && wayPoints.Length > 1);
            
            wayPointTransforms = wayPoints.Select(t => t.transform).ToArray();
        }
    }


    protected override void Start()
    {
        base.Start();

        #region AI

        m_PlayerPosition = Vector3.zero;
        m_IsPatrol = true;
        m_playerInAttackRange = false;
        m_playerInSight = false;
        m_PlayerNear = false;
        m_WaitTime = startWaitTime;                 //  Set the wait time variable that will change
        m_TimeToRotate = timeToRotate;

        m_CurrentWayPointIndex = 0;                 //  Set the initial waypoint
        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speedWalk;             //  Set the navemesh speed with the normal speed of the enemy
        navMeshAgent.SetDestination(wayPointTransforms[m_CurrentWayPointIndex].position);    //  Set the destination to the first waypoint
        #endregion
    }


    protected override void Update()
    {
        if (!_stop)
        {
            base.Update();
            
            UpdateView();

            UpdateMovement(); // for animation

            if (!m_IsPatrol)
            {
                if(m_playerInAttackRange)
                {
                    Attacking();
                }
                else // not in range
                {
                    Chasing();
                }
            }
            else
            {
                Patroling();
            }
        }
        else // stop
        {
            StopMovement();
        }
    }

    private void Attacking()
    {
        Debug.Log("attacking");

        LookAtPlayer(m_PlayerPosition);
        AttackPlayer();
    }

    // shotPlayer
    private void AttackPlayer()
    {
        GetCurrentWeapon.TriggerDown = true;
    }

    private void LookAtPlayer(Vector3 m_PlayerPosition)
    {
        var OrientationSpeed = 10f;

        Vector3 lookDirection = Vector3.ProjectOnPlane(m_PlayerPosition - transform.position, Vector3.up).normalized;
        if (lookDirection.sqrMagnitude != 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation =
                Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * OrientationSpeed);
        }
    }

    private void Chasing()
    {
        //  The enemy is chasing the player
        m_PlayerNear = false;                       //  Set false that hte player is near beacause the enemy already sees the player
        m_playerLastPosition = Vector3.zero;          //  Reset the player near position

        if (!m_playerInAttackRange)
        {
            Move(speedRun);
            navMeshAgent.SetDestination(m_PlayerPosition);          //  set the destination of the enemy to the player location
        }
        else
        {
            Stop();
        }

            //  Control if the enemy arrive to the player location
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)  
        {
            //  Check if the enemy is not near to the player,
            //  returns to patrol after the wait time delay
            if (m_WaitTime <= 0 
                && !m_playerInAttackRange
                && GetPlayerDistance() >= 6f)// return to patrol range
            {
                m_IsPatrol = true;
                m_PlayerNear = false;

                Move(speedWalk);

                m_TimeToRotate = timeToRotate;
                m_WaitTime = startWaitTime;
                navMeshAgent.SetDestination(wayPointTransforms[m_CurrentWayPointIndex].position);
            }
            else
            {
                // GetPlayerDistance() > 2.5f; // chase stop range
                if (GetPlayerDistance() >= 2.5f)
                {
                    Stop();
                }
                 //  Wait if the current position is not
                 //  the player position
                m_WaitTime -= Time.deltaTime;
            }
        }
    }

    private float GetPlayerDistance()
    {
       return  Vector3.Distance(transform.position,
                    _playerTransform.position);
    }

    private void Patroling()
    {
        if (m_PlayerNear)
        {
            //  Check if the enemy detect near the player, so the enemy will move to that position
            if (m_TimeToRotate <= 0)
            {
                Move(speedWalk);
                MoveToPlayer(m_playerLastPosition);
            }
            else
            {
                //  The enemy wait for a moment and then go to the last player position
                Stop();
                m_TimeToRotate -= Time.deltaTime;
            }
        }
        else
        {
            m_PlayerNear = false;           //  The player is no near when the enemy is platroling
            m_playerLastPosition = Vector3.zero;
            navMeshAgent.SetDestination(wayPointTransforms[m_CurrentWayPointIndex].position);    //  Set the enemy destination to the next waypoint
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                //  If the enemy arrives to the waypoint position then wait for a moment and go to the next
                if (m_WaitTime <= 0)
                {
                    NextPoint();
                    Move(speedWalk);
                    m_WaitTime = startWaitTime;
                }
                else
                {
                    Stop();
                    m_WaitTime -= Time.deltaTime;
                }
            }
        }
    }

    public void NextPoint()
    {
        m_CurrentWayPointIndex = (m_CurrentWayPointIndex + 1) % wayPointTransforms.Length;
        navMeshAgent.SetDestination(wayPointTransforms[m_CurrentWayPointIndex].position);
    }

    void Stop()
    {
        navMeshAgent.isStopped = true;
        navMeshAgent.speed = 0;
    }

    void Move(float speed)
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speed;
    }

    void MoveToPlayer(Vector3 player)
    {
        navMeshAgent.SetDestination(player);
        if (Vector3.Distance(transform.position, player) <= 0.3)
        {
            if (m_WaitTime <= 0)
            {
                m_PlayerNear = false;
                Move(speedWalk);
                navMeshAgent.SetDestination(wayPointTransforms[m_CurrentWayPointIndex].position);
                m_WaitTime = startWaitTime;
                m_TimeToRotate = timeToRotate;
            }
            else
            {
                Stop();
                m_WaitTime -= Time.deltaTime;
            }
        }
    }

    void UpdateView()
    {
        //  Make an overlap sphere around the enemy
        //  to detect the playermask
        Collider[] playerInRange = Physics
            .OverlapSphere(transform.position, 
            viewRadius, 
            playerMask);   

        for (int i = 0; i < playerInRange.Length; i++)
        {
            Transform player = playerInRange[i].transform;
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
            {
                float dstToPlayer = Vector3.Distance(transform.position, player.position);          //  Distance of the enmy and the player
                if (!Physics.Raycast(transform.position, dirToPlayer, dstToPlayer, obstacleMask))
                {
                    m_playerInSight = true;             //  The player has been seeing by the enemy and then the enemy starts to chasing the player
                    m_IsPatrol = false;                 //  Change the state to chasing the player

                    if (GetPlayerDistance() <= attackRange)
                    {
                        m_playerInAttackRange = true;
                    }
                    else
                    {
                        m_playerInAttackRange = false;
                    }
                }
                else
                {
                    /*
                     *  If the player is behind a obstacle the player position will not be registered
                     * */
                    m_playerInSight = false;
                }
            }
            if (Vector3.Distance(transform.position, player.position) > viewRadius)
            {
                /*
                 *  If the player is further than the view radius, then the enemy will no longer keep the player's current position.
                 *  Or the enemy is a safe zone, the enemy will no chase
                 * */
                //  Change the sate of chasing
                m_playerInSight = false;        
            }

            if (m_playerInSight)
            {
                m_PlayerPosition = player.transform.position;     
            }
            else
            {
                m_playerInAttackRange = false;
            }
        }
    }

    protected override void UpdateMovement()
    {
        // move
        _speed = navMeshAgent.speed;
        if(_speed > 0.01)
        {
            _hzInput = 0;
            _vInput = 1;
        }

        // jump
        m_Grounded = true;
        
        // Fall down / gravity
    }

    protected void StopMovement()
    {
        // move
        _speed = 0;
        _hzInput = 0;
        _vInput = 0;

        // jump

        // Fall down / gravity
    }

    public override void InitWeapon(WeaponItem weaponItem)
    {
        base.InitWeapon(weaponItem);

        attackRange = weaponItem.Weapon1P.GetWeaponRange();
        //foreach (var item in _weapon1Ps)
        //{
        //    item.Value.Hide();
        //}
    }

    void OnDrawGizmosSelected()
    {
        var fov = this;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.viewRadius);
        Vector3 viewAngleA = fov.DirFromAngle(-fov.viewAngle / 2, false);
        Vector3 viewAngleB = fov.DirFromAngle(fov.viewAngle / 2, false);

        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);

        //Handles.color = Color.red;
        //foreach (Transform visibleTarget in fow.visibleTargets)
        //{
        //    Handles.DrawLine(fow.transform.position, visibleTarget.position);
        //}
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public override Ray GetShotRay()
    {
        // spread
        float spreadRatio = 0;
        Vector2 spread = spreadRatio * UnityEngine.Random.insideUnitCircle;

        // ray from camera1P
        //var muzzlePosition = WeaponPosition3P;
        var muzzlePosition = GetCurrentWeapon3P.Muzzle;
        if (muzzlePosition == null)
        {
            muzzlePosition = WeaponPosition3P;
        }
        Ray ray = new Ray(muzzlePosition.position,
                _playerTransform.position); // m_PlayerPosition

        Debug.DrawLine(ray.origin, ray.direction, 
            Color.blue, 5f);

        return ray;
    }
    // End
}
