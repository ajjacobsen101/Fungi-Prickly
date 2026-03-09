using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{

    private PlayerController player;

    [SerializeField] GameObject sporeAttack;

    

    bool weaponToggle = false;

    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();
    }
    public void Jump()
    {
        Debug.Log("Called Jump Function from animation Event");

        if (player != null)
            player.ApplyJumpForce();
        else
            Debug.Log("Player is null");
    }

    public void EndAttackAnimation()
    {
        Debug.Log("SporeAttackEnded called");
        player.EndAttack();
    }

    public void WeaponToggle()
    {
        //weaponToggle = !weaponToggle;
        //player.SheathWeapon(weaponToggle);
    }

    public void PlaySporeAttack()
    {
        Instantiate(sporeAttack,player.transform);
    }
}
