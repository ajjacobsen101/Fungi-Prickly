using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;
    [Header("Pause Menu Settings")]
    [SerializeField] GameObject pauseMenu;
    

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void SetPauseUI(bool status)
    {
        pauseMenu.SetActive(status);
    }

    public void ResumeGame()
    {
        GameManager.instance.ChangeState(GameState.Playing);
    }

    public void QuitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
 
}
