using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Esc or P pauses the game. The panel has volume sliders and buttons to resume,
// go back to the main menu or quit.
public class PauseMenu : MonoBehaviour
{
    public static bool paused = false;

    public GameObject panel;
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        paused = false;
        panel.SetActive(false);
        musicSlider.value = GameState.instance.musicVolume;
        sfxSlider.value = GameState.instance.sfxVolume;
        musicSlider.onValueChanged.AddListener(GameState.instance.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(GameState.instance.SetSfxVolume);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (paused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        paused = true;
        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        paused = false;
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void MainMenu()
    {
        Resume();
        SceneManager.LoadScene("Menu");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
