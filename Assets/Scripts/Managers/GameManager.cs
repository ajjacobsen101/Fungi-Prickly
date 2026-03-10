using System;
using UnityEngine;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Cutscene,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance {  get; private set; }
    public GameState CurrentState { get; private set; }

    public bool IsPaused() => CurrentState == GameState.Paused;

   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        
    }

    private void OnEnable()
    {
        EventManager.OnStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        EventManager.OnStateChanged -= HandleGameStateChanged;
    }

    public void ChangeState(GameState newState)
    {
        if(CurrentState == newState) return;

        CurrentState = newState;
        EventManager.OnStateChanged?.Invoke(CurrentState);
        Debug.Log("Game State changed to: " + CurrentState);
    }

    void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                UiManager.instance.SetPauseUI(false);
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                UiManager.instance.SetPauseUI(true);
                break;
            case GameState.MainMenu:
                Time.timeScale = 1f;
                break;
            case GameState.Cutscene:

                break;
        }
    }
}
