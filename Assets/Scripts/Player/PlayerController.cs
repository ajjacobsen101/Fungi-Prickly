using JetBrains.Annotations;
using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

#region STATE INTERFACES & MACHINE

public interface IPlayerState
{
    void Enter();
    void Update();
    void Exit();
}

public class PlayerStateMachine
{
    public IPlayerState CurrentState { get; private set; }

    public void ChangeState(IPlayerState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }
}

public abstract class PlayerState : IPlayerState
{
    protected PlayerController player;

    protected PlayerState(PlayerController player)
    {
        this.player = player;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}

public class PlayerStates
{
    public IdleState Idle;
    public MoveState Move;
    public JumpState Jump;
    public SwordAttackState SwordAttack;
    public SpecialAttackState SpecialAttack;
    public NoActionState NoAction;

    public PlayerStates(PlayerController player)
    {
        Idle = new IdleState(player);
        Move = new MoveState(player);
        Jump = new JumpState(player);
        SwordAttack = new SwordAttackState(player);
        SpecialAttack = new SpecialAttackState(player);
        NoAction = new NoActionState(player);
    }
}

#endregion

#region PLAYER STATES

public class IdleState : PlayerState
{
    public IdleState(PlayerController player) : base(player) { }

    public override void Enter()
    {
       
        //player.anim.SetFloat("Speed", 0f);
    }

    public override void Update()
    {
       

        if (player.MoveInput.sqrMagnitude > 0.01f)
            player.LocomotionSM.ChangeState(player.States.Move);

        if (player.JumpPressed)
            player.LocomotionSM.ChangeState(player.States.Jump);
    }
}

public class MoveState : PlayerState
{
    public MoveState(PlayerController player) : base(player) { }

    public override void Enter() { }

    public override void Update()
    {
        

        player.MovePlayer();

        if (player.MoveInput.sqrMagnitude < 0.01f)
            player.LocomotionSM.ChangeState(player.States.Idle);

        if (player.JumpPressed)
            player.LocomotionSM.ChangeState(player.States.Jump);
    }
}

public class JumpState : PlayerState
{
    public JumpState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        
        player.anim.SetTrigger("Jump");
        player.ApplyJumpForce();
    }

    public override void Update()
    {
        

        player.MovePlayer();

        if (player.controller.isGrounded)
        {
            if (player.MoveInput.sqrMagnitude > 0.01f)
                player.LocomotionSM.ChangeState(player.States.Move);
            else
                player.LocomotionSM.ChangeState(player.States.Idle);
        }
    }
}

public class SwordAttackState : PlayerState
{
    public SwordAttackState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        
        player.PlayNextAttack(); 
    }

    public override void Exit()
    {
       
    }
}

public class SpecialAttackState : PlayerState
{
    public SpecialAttackState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        
        player.anim.SetTrigger("SporeAttack");
        player.isAttacking = true;
        player.UseMana(player.stats.sporeAttackManaCost);
        player.LocomotionSM.ChangeState(player.States.Idle);
    }

    public override void Exit()
    {
        player.isAttacking = false;
    }
}

public class NoActionState : PlayerState
{
    public NoActionState(PlayerController player) : base(player) { }

    public override void Enter() {  }
    public override void Update() { }
}

//public class InteractState : PlayerState
//{
//    private Interactables target;

//    public InteractState(PlayerController player, Interactables target)
//        : base(player)
//    {
//        this.target = target;
//    }

//    public override void Enter()
//    {
        
//       // player.anim.SetTrigger("Interact");
//        target.Interact(player);
//    }

//    public override void Exit() { }
//}

#endregion

#region PLAYER CONTROLLER


public enum PlayerType
{
    Fungi,
    Prickly
}

public class PlayerController : MonoBehaviour
{

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private int jumpMax;
    [SerializeField] private float gravity;
    [SerializeField] private float fallMultiplier;

    [Header("References")]
    [SerializeField] public Camera cam;
    public Animator anim;
    [SerializeField] private Transform weaponHoldTransform;

    [Header("Combat")]
    [SerializeField] private float combatTimeout;


  




    [Header("Cinemachine")]
    [SerializeField] private float idleObitSpeed;
    [SerializeField] private float idleDelay;

    [Header("Event Channels")]
    [SerializeField] FloatEventChannelSO healthChangedEvent;
    [SerializeField] FloatEventChannelSO ManaChangedEvent;

   
   

    
    
    private InputAction move;
    private InputAction jump;
    private InputAction attack;
    private InputAction interact;
    private InputAction modifierAttack;
    private InputAction specialAttack1;
    private InputAction look;
    private InputAction zoom;
    private InputAction pause;

    public CharacterController controller;
    private CinemachineOrbitalFollow orbital;
    private Vector2 scrollDelta;
    private float targetZoom;
    private float currentZoom;
    private float verticalVelocity;
    private float combatTimer;
    private int jumpCount;
    private int currentAttackIndex;
    private float idleTimer; 

    [SerializeField] bool hasWeaponEquipped;
    [HideInInspector] public bool isAttacking;

    private bool modifierHeld;
   // private Interactables interactable;

    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool AttackPressed { get; private set; }

     public PlayerType PlayerType { get; private set; }

    public PlayerStates States;
    public PlayerStats stats;
    public PlayerStateMachine LocomotionSM;
    public PlayerStateMachine ActionSM;

    private PlayerInput playerInput;
    

    #region UNITY CALLBACKS

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        weaponHoldTransform = FindWeaponSocket();
        playerInput = GetComponent<PlayerInput>();

       

        LocomotionSM = new PlayerStateMachine();
        ActionSM = new PlayerStateMachine();

        States = new PlayerStates(this);

        LocomotionSM.ChangeState(States.Idle);
        ActionSM.ChangeState(States.NoAction);

        RegenerateHealth(stats.HealthMax);
        RegenerateMana(stats.manaMax);
        cam = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        CinemachineCamera freeLook = GetComponentInChildren<CinemachineCamera>();
        orbital = freeLook.GetComponent<CinemachineOrbitalFollow>();

        EventManager.OnPlayerRegistered?.Invoke(this.transform);
    }

   

    private Transform FindWeaponSocket()
    {
        WeaponTransform socket = GetComponentInChildren<WeaponTransform>(true);
        return socket != null ? socket.transform : null;
    }

    private void OnEnable()
    {
        move = playerInput.actions["Move"];
        jump = playerInput.actions["Jump"];
        attack = playerInput.actions["Attack"];
        interact = playerInput.actions["Interact"];
        modifierAttack = playerInput.actions["ModifierAttack"];
        specialAttack1 = playerInput.actions["SpecialAttack1"];
        look = playerInput.actions["Look"];
        zoom = playerInput.actions["Zoom"];
        pause = playerInput.actions["Pause"];

        pause.performed += ctx => TogglePause();

      
        jump.started += ctx => JumpPressed = true;
       
        interact.performed += ctx =>
        {
           // if (interactable == null) return;
            if (ActionSM.CurrentState != States.NoAction) return;

           // ActionSM.ChangeState(new InteractState(this, interactable));
        };
        attack.performed += OnAttack;
        specialAttack1.performed += OnSpecialAttack;

        modifierAttack.started += ctx => modifierHeld = true;
        modifierAttack.canceled += ctx => modifierHeld = false;

        

        playerInput.actions.Enable();


       
    }

    

    private void OnDisable()
    {
        
        playerInput.actions.Disable();
    }


    public void InitializePlayerType(PlayerType type)
    {
        PlayerType = type;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Interactables i = other.GetComponent<Interactables>();

        //if (!i) return;

        //if (i.isEquipped) return;

        //interactable = i;
    }

    private void Update()
    {
        if (GameManager.instance.IsPaused()) return;
        MoveInput = move.ReadValue<Vector2>();
        JumpPressed = jump.WasPressedThisFrame();
        AttackPressed = attack.WasPressedThisFrame();

        LocomotionSM.Update();
        ActionSM.Update();
        anim.SetFloat("Speed", MoveInput.magnitude);

        if (hasWeaponEquipped && !isAttacking && combatTimer > 0)
            UpdateCombatTimer();

        HandleIdleOrbit();

       
    }


    #endregion

    #region GameStates

    void TogglePause()
    {
        GameManager.instance.ChangeState(GameState.Paused);
    }

    

    #endregion

    #region Stat Manipulation Events

    public void TakeDamage(float damage)
    {
        stats.currentHealth -= damage;
        stats.currentHealth = Mathf.Max(stats.currentHealth, 0);
        Debug.Log(stats.currentHealth.ToString());
       
            healthChangedEvent.RaiseDouble(stats.currentHealth, stats.HealthMax, PlayerType);
       
      
    }

    public void RegenerateHealth(float amount)
    {
        stats.currentHealth += amount;


        stats.currentHealth = Math.Max(stats.currentHealth, stats.HealthMax);
        Debug.Log(stats.currentHealth.ToString());

        healthChangedEvent.RaiseDouble(stats.currentHealth, stats.HealthMax, PlayerType);
    }

    public void UseMana(float amount)
    {
        stats.currentMana -= amount;
        stats.currentMana = Mathf.Max(stats.currentMana, 0);
       
            ManaChangedEvent.RaiseDouble(stats.currentMana, stats.manaMax, PlayerType);
       
    }

    public void RegenerateMana(float amount)
    {
        stats.currentMana += amount;
        stats.currentMana = Mathf.Max(stats.currentMana, stats.manaMax);
        
            ManaChangedEvent.RaiseDouble(stats.currentMana, stats.manaMax, PlayerType);
       

    }

    #endregion


    #region MOVEMENT

   
    public void MovePlayer()
    {
        Vector3 moveDir = GetCameraRight(cam) * MoveInput.x + GetCameraForward(cam) * MoveInput.y;
        Vector3 horizontalMove = moveDir.normalized * moveSpeed;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
                jumpCount = 0;
            }
        }

        verticalVelocity += gravity * (verticalVelocity < 0 ? fallMultiplier : 1f) * Time.deltaTime;

        Vector3 motion = horizontalMove;
        motion.y = verticalVelocity;
        controller.Move(motion * Time.deltaTime);

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    public void ApplyJumpForce()
    {
        if (!JumpPressed) return;
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        jumpCount++;
        JumpPressed = false;
    }

    private Vector3 GetCameraForward(Camera cam)
    {
        Vector3 forward = cam.transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera cam)
    {
        Vector3 right = cam.transform.right;
        right.y = 0f;
        return right.normalized;
    }

    #endregion

    #region COMBAT

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (ActionSM.CurrentState != States.NoAction || stats.currentHealth <= 0)
            return;

        combatTimer = combatTimeout;

        if (modifierHeld)
        {
            //if (hasWeaponEquipped) SheathWeapon(false);
            ActionSM.ChangeState(States.SpecialAttack);
            Debug.Log("Special Attack (controller) started");
        }
       

       
    }

    public void OnSpecialAttack(InputAction.CallbackContext context)
    {
        if (ActionSM.CurrentState != States.NoAction || stats.currentHealth <= 0)
            return;

        if (hasWeaponEquipped) SheathWeapon(false);
        ActionSM.ChangeState(States.SpecialAttack);
    }

    

    public void PlayNextAttack()
    {
        //if (currentEquippedCombo == null || currentEquippedCombo.attackTriggers.Length == 0) return;

       

        //combatEvent?.RaiseAttackStarted(PlayerType);
        
        //anim.SetTrigger(currentEquippedCombo.attackTriggerName);
        //anim.SetBool("IsNotAttacking", false);
        //int trigger = currentEquippedCombo.attackTriggers[currentAttackIndex].comboIndex;
        //anim.SetInteger("ComboIndex", trigger);

        //currentAttackIndex = (currentAttackIndex + 1) % currentEquippedCombo.attackTriggers.Length;
    }

    public void EndAttack()
    {
       

        //if (ActionSM.CurrentState != States.SwordAttack &&
        //ActionSM.CurrentState != States.SpecialAttack)
        //    return;

        //isAttacking = false;
        //anim.SetBool("IsNotAttacking", true);
        //ActionSM.ChangeState(States.NoAction);
        //combatEvent?.RaiseAttackEnded(PlayerType);
    }

    private void UpdateCombatTimer()
    {
        combatTimer -= Time.deltaTime;
        if (combatTimer <= 0f)
            ExitCombat();
    }

    private void ExitCombat()
    {
        currentAttackIndex = 0;
        SheathWeapon(false);
      
    }

    public void SheathWeapon(bool status)
    {
        if (hasWeaponEquipped == status) return;

        if (status)
            anim.SetTrigger("UnSheathWeapon");
        else
            anim.SetTrigger("SheathWeapon");

            weaponHoldTransform.gameObject.SetActive(status);
        hasWeaponEquipped = weaponHoldTransform.gameObject.activeSelf;
        anim.SetBool("HoldingWeapon", hasWeaponEquipped);
       
    }

    public void SelectRandomCombo()
    {
        //if (currentEquippedCombo == null || currentEquippedCombo.attackTriggers.Length == 0) return;
        //currentAttackIndex = UnityEngine.Random.Range(0, currentEquippedCombo.attackTriggers.Length);
    }


    #endregion

    #region IDLE ORBIT

    private void HandleIdleOrbit()
    {
        if (MoveInput.sqrMagnitude > 0.01f || JumpPressed || AttackPressed)
        {
            idleTimer = 0f;
            anim.SetBool("NoInput", false);
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDelay)
            {
                transform.Rotate(Vector3.up, idleObitSpeed * Time.deltaTime, Space.World);
                anim.SetBool("NoInput", true);
            }
        }
    }

    #endregion
}
#endregion

#region PlayerStats
[System.Serializable]
public class PlayerStats
{
    public float HealthMax;
    public float currentHealth;
    public float manaMax;
    public float currentMana;
    public float strength;
    public float defense;
    public int level;
    public float specialAbilityCooldown;
    public float sporeAttackManaCost;
}
#endregion