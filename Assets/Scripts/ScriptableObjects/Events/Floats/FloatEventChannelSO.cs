using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/Float Event Channel")]
public class FloatEventChannelSO : ScriptableObject
{
    public event Action<float, PlayerType> OnEventRaised;
    public event Action<float, float, PlayerType> OnEventRaisedDouble;
    public event Action<float, float, float, PlayerType> OnEventRaisedTriple;

    public void Raise(float value, PlayerType playerType)
    {
        OnEventRaised?.Invoke(value, playerType);
    }

    public void RaiseDouble(float value, float secondValue, PlayerType playerType)
    {
        OnEventRaisedDouble?.Invoke(value, secondValue, playerType);
    }

    public void RaiseTriple(float value, float secondValue, float thirdValue, PlayerType playerType)
    {
        OnEventRaisedTriple?.Invoke(value, secondValue, thirdValue, playerType);
    }
}
