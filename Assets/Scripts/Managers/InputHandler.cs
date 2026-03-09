using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour, IInputAxisController
{
    public InputAction horizontal;
    public InputAction vertical;

    public bool ControllersAreValid()
    {
        throw new System.NotImplementedException();
    }

    public float GetAxisValue(int axis)
    {
        switch (axis)
        {
            case 0: return horizontal.ReadValue<Vector2>().x;
            case 1: return horizontal.ReadValue<Vector2>().y;
            case 2: return vertical.ReadValue<float>();
        }
        return 0;
    }

    public void SynchronizeControllers()
    {
        throw new System.NotImplementedException();
    }
}
