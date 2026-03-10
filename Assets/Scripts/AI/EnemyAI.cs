using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Animator anim;
    public Transform player;

    [Header("Stats")]
    public EnemyStats stats;

    [Header("Combos")]
    public List<EnemyCombo> combos;

    [Header("Patrol")]
    public float patrolRadius;
    public float patrolPointTolerance;
    public float minPatrolWaitTime;
    public float maxPatrolWaitTime;
    private float patrolWaitTimer;
    private bool waitingAtPatrolPoint;


    Vector3 patrolTarget;

    EnemyCombo currentCombo;
    int comboIndex;
    Coroutine comboRoutine;

    float currentHealth;

    // State Machines
    public EnemyStateMachine movementSM;
    public EnemyStateMachine combatSM;

    // State Container
    public EnemyStates States;

    public List<Transform> activePlayers = new List<Transform>();
    Transform newPlayer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

      //  player = GameObject.FindGameObjectWithTag("Player").transform;

        movementSM = new EnemyStateMachine();
        combatSM = new EnemyStateMachine();

        States = new EnemyStates(this);

        currentHealth = stats.maxHealth;
        agent.speed = stats.moveSpeed;
    }

    void Start()
    {
        if (!agent.isOnNavMesh)
            Debug.LogError("Agent not on NavMesh!");

        movementSM.ChangeState(States.Patrol);
        combatSM.ChangeState(States.NoAction);
    }

    private void OnEnable()
    {
        EventManager.OnPlayerRegistered += RegisterPlayer;
        EventManager.OnPlayerUnregistered += UnregisterPlayer;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerRegistered -= RegisterPlayer;
        EventManager.OnPlayerUnregistered -= UnregisterPlayer;
    }

    void Update()
    {
        movementSM.Update();
        combatSM.Update();

        if (agent.velocity.sqrMagnitude > 0) {
            anim.SetBool("NoInput", false);
            anim.SetFloat("Speed", agent.velocity.magnitude / stats.moveSpeed);
        }
        else if(combatSM.CurrentState == States.NoAction)
        {
            anim.SetBool("NoInput", true);
        }
    }

    void RegisterPlayer(Transform player)
    {
        Debug.Log("Player Registered: " + player.name);
        if (!activePlayers.Contains(newPlayer))
        {
            activePlayers.Add(player);

            if (player == null)
                player = newPlayer;
        }
    }

    void UnregisterPlayer(Transform player)
    {
        if (activePlayers.Contains(player))
        {
            activePlayers.Remove(player);
        }
    }

    // -----------------------------
    // Movement
    // -----------------------------

    public void MoveTowardPlayer()
    {
        if (player == null) return;

        agent.SetDestination(player.position);
        
    }

    public Transform GetClosestPlayer()
    {
        if (activePlayers.Count == 0) return null;

        Transform closest = null;
        float closestDist = Mathf.Infinity;

        for (int i = activePlayers.Count - 1; i >= 0; i--)
        {
            Transform p = activePlayers[i];

            if (p == null)
            {
                activePlayers.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(transform.position, p.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = p;
            }
        }

        return closest;
    }

    public Vector3 GetRandomPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection.y = 0;
        randomDirection += transform.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }

    public void MoveToRandomPatrolPoint()
    {
        patrolTarget = GetRandomPatrolPoint();

        Debug.Log("New patrol target: " + patrolTarget);
        agent.SetDestination(patrolTarget);
    }

    public void Patrol()
    {
        if (waitingAtPatrolPoint)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0f)
            {
                // Finished waiting, go to next patrol point
                waitingAtPatrolPoint = false;
                MoveToRandomPatrolPoint();
            }
            return;
        }

        // If agent has reached the patrol point
        if (!agent.pathPending && agent.remainingDistance <= patrolPointTolerance)
        {
            // Start waiting
            waitingAtPatrolPoint = true;
            patrolWaitTimer = Random.Range(minPatrolWaitTime, maxPatrolWaitTime);

            // Stop agent so velocity = 0 → triggers NoInput
            agent.isStopped = true;
        }
        else
        {
            // Make sure agent is moving
            agent.isStopped = false;
        }
    }

    public bool CanSeePlayer()
    {
        player = GetClosestPlayer();

        if (player == null)
        {
            Debug.Log("No player found");
            return false;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        Debug.Log("Distance to player: " + dist);

        return dist <= stats.detectionRange;
    }

    public bool IsInAttackRange()
    {
        if (player == null) return false;

        return Vector3.Distance(transform.position, player.position) <= stats.attackRange;
    }

    // -----------------------------
    // Combat
    // -----------------------------

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        InterruptCombo();
        combatSM.ChangeState(States.Stunned);
    }

    void Die()
    {
        agent.isStopped = true;
       // anim.SetTrigger("Die");
        enabled = false;
    }

    public void SelectRandomCombo()
    {
        if (combos.Count == 0) return;

        currentCombo = combos[Random.Range(0, combos.Count)];
        comboIndex = 0;
    }
    public void StartCombo()
    {
        if (comboRoutine != null)
            StopCoroutine(comboRoutine);

       // comboRoutine = StartCoroutine(ExecuteCombo());
    }

    IEnumerator ExecuteCombo()
    {
        while (comboIndex < currentCombo.animationTriggers.Length)
        {
            anim.SetTrigger(currentCombo.animationTriggers[comboIndex]);

            float delay = currentCombo.hitDelays[comboIndex];

            yield return new WaitForSeconds(delay);

            comboIndex++;
        }

        combatSM.ChangeState(States.NoAction);
    }
    public void InterruptCombo()
    {
        if (comboRoutine != null)
            StopCoroutine(comboRoutine);

        comboIndex = 0;
    }
}

public class EnemyIdleState : EnemyState
{
    public EnemyIdleState(EnemyAI enemy) : base(enemy) { }

    public override void Update()
    {
        if (enemy.CanSeePlayer())
        {
            enemy.movementSM.ChangeState(enemy.States.Chase);
            enemy.combatSM.ChangeState(enemy.States.NoAction);
        }
    }
}
public class EnemyPatrolState : EnemyState
{
    public EnemyPatrolState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.agent.isStopped = false;
        enemy.MoveToRandomPatrolPoint();
        enemy.combatSM.ChangeState(enemy.States.NoAction);
    }

    public override void Update()
    {
        Debug.Log("Patrolling");
        enemy.Patrol();


        if (enemy.CanSeePlayer())
        {
            enemy.movementSM.ChangeState(enemy.States.Chase);
        }
    }
}

public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.agent.isStopped = false; // make sure agent moves
    }

    public override void Update()
    {
        Transform target = enemy.GetClosestPlayer();
        if (target == null)
        {
            // No players left → return to patrol
            enemy.movementSM.ChangeState(enemy.States.Patrol);
            return;
        }

        float dist = Vector3.Distance(enemy.transform.position, target.position);

        if (dist > enemy.stats.detectionRange)
        {
            // Player out of detection → return to patrol
            enemy.movementSM.ChangeState(enemy.States.Patrol);
            return;
        }

        // Move toward the closest player
        enemy.MoveTowardPlayer();

        // Start attack if in range
        if (dist <= enemy.stats.attackRange)
        {
            enemy.combatSM.ChangeState(enemy.States.Attack);
        }
    }

    public override void Exit()
    {
        // nothing special
    }
}

public class EnemyNoActionState : EnemyState
{
    public EnemyNoActionState(EnemyAI enemy) : base(enemy) { }

    public override void Update()
    {
        if (enemy.IsInAttackRange())
        {
            enemy.combatSM.ChangeState(enemy.States.Attack);
        }
    }
}

public class EnemyAttackState : EnemyState
{
    float attackTimer;

    public EnemyAttackState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.agent.isStopped = true;

        enemy.SelectRandomCombo();

        enemy.StartCombo();
    }

    public override void Update()
    {
       
    }

    public override void Exit()
    {
        enemy.agent.isStopped = false;
        enemy.InterruptCombo();
    }
}

public class EnemyStunnedState : EnemyState
{
    float stunTimer;

    public EnemyStunnedState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        stunTimer = enemy.stats.stunDuration;

        enemy.agent.isStopped = true;
       // enemy.anim.SetTrigger("Stunned");
    }

    public override void Update()
    {
        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0f)
        {
            enemy.agent.isStopped = false;
            enemy.combatSM.ChangeState(enemy.States.NoAction);
        }
    }
}


public class EnemyStates
{
    public EnemyIdleState Idle;
    public EnemyPatrolState Patrol;
    public EnemyChaseState Chase;

    public EnemyNoActionState NoAction;
    public EnemyAttackState Attack;
    public EnemyStunnedState Stunned;

    public EnemyStates(EnemyAI enemy)
    {
        Idle = new EnemyIdleState(enemy);
        Patrol = new EnemyPatrolState(enemy);
        Chase = new EnemyChaseState(enemy);

        NoAction = new EnemyNoActionState(enemy);
        Attack = new EnemyAttackState(enemy);
        Stunned = new EnemyStunnedState(enemy);
    }
}

public abstract class EnemyState : IEnemyState
{
    protected EnemyAI enemy;

    public EnemyState(EnemyAI enemy)
    {
        this.enemy = enemy;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}

public class EnemyStateMachine
{
    public IEnemyState CurrentState { get; private set; }
    public void ChangeState(IEnemyState state)
    {
        CurrentState?.Exit();
        CurrentState = state;
        CurrentState.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }
}

public interface IEnemyState
{
    void Enter();
    void Update();
    void Exit();
}