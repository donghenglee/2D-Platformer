using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused= false;

    public GameObject pauseMenuUI;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
    public void Resume()
    {
        isPaused=false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }
    void Pause()
    {
        isPaused=true;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }
    public void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f;
    }
    public void QuitGame()
    {
        Debug.Log("quitgame");
        Application.Quit();
    }
}
