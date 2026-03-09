using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{

    [SerializeField] Slider healthBar;
    [SerializeField] Slider manaBar;

    [Header("Events")]
    [SerializeField] FloatEventChannelSO healthChangedEvent;
    [SerializeField] FloatEventChannelSO ManaChangedEvent;

    [SerializeField] PlayerType myPlayerType;


    public void Initialize(PlayerType type)
    {
        myPlayerType = type;
    }

    private void OnEnable()
    {
        healthChangedEvent.OnEventRaisedDouble += UpdateHealth;
        ManaChangedEvent.OnEventRaisedDouble += UpdateMana;

       

    }

    private void OnDisable()
    {
        healthChangedEvent.OnEventRaisedDouble -= UpdateHealth;
        ManaChangedEvent.OnEventRaisedDouble -= UpdateMana;

       
    }

    void UpdateHealth(float current, float max, PlayerType playerType)
    {
        if (playerType != myPlayerType) return;
        healthBar.value = current / max;
    }

    void UpdateMana(float current, float max, PlayerType playerType)
    {
        if (playerType != myPlayerType) return;
        manaBar.value = current / max;
    }

    
    
}
