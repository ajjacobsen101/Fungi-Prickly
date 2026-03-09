using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainCamToggle : MonoBehaviour
{
    private PlayerInputManager playerInputManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInputManager = FindFirstObjectByType<PlayerInputManager>();
    }

    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += ToggleThis;
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= ToggleThis;
    }
    private void ToggleThis(PlayerInput input)
    {
        this.gameObject.SetActive(false);
    }
}
