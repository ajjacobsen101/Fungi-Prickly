using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject playButton;
    public GameObject optionsButton;
    public GameObject quitGameButton;
    public GameObject creditsButton;

    private bool hasSelected = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (hasSelected) return;

        if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                if (control is ButtonControl button && button.wasPressedThisFrame)
                {
                    EventSystem.current.SetSelectedGameObject(playButton);
                    hasSelected = true;
                    break;
                }
            }
        }
        else if (Keyboard.current != null || Mouse.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void ChoosePlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void ChooseQuitGame()
    {
        Application.Quit();
    }

}
