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

    public event Action<GameState> OnStateChanged;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        
    }

    public void ChangeState(GameState newState)
    {
        if(CurrentState == newState) return;

        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
        Debug.Log("Game State changed to: " + CurrentState);
    }


}
