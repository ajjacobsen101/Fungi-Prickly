using UnityEngine;

public class MainMenuCharacterController : MonoBehaviour
{
   // [SerializeField] GameObject weapon;
    bool isSwordActive;
    private void Awake()
    {
        //weapon.SetActive(false);
        //isSwordActive = weapon.activeSelf;
    }

    public void WeaponToggle()
    {
        //isSwordActive = !isSwordActive;
        //weapon.SetActive(isSwordActive);
    }
}
