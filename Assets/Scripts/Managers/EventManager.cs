using System;
using UnityEngine;

public static class EventManager
{

    public static Action<Transform> OnPlayerRegistered;
    public static Action<Transform> OnPlayerUnregistered;


    public static Action<GameState> OnStateChanged;

}
