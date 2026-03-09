using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;


public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Animator anim;
    public Transform player;

    [Header("Stats")]
    public EnemyStats stats;

    [Header("Combos")]
    

    public EnemyStateMachine movementSM;
    public EnemyStateMachine combatSM;
    public EnemyState States;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveTowardPlayer()
    {

    }

    public bool IsInAttackRange()
    {
        return false;
    }
}



public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyAI enemy) : base(enemy)
    {

    }

    public override void Update()
    {
        enemy.MoveTowardPlayer();

        if (enemy.IsInAttackRange())
        {
            
        }
    }
}


public abstract class EnemyState : IEnemyState
{
    protected EnemyAI enemy;

    protected EnemyState(EnemyAI enemy)
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