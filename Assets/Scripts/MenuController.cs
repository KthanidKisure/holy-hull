using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Used on the Menu, Victory and GameOver screens: press Space to move on.
public class MenuController : MonoBehaviour
{
    public string nextScene = "Level1";
    public bool resetGame = true;
    public Text blinkText;
    public Text bestText;

    void Start()
    {
        Time.timeScale = 1f;        if (bestText != null && GameState.instance != null)
        {
            bestText.text = "HIGH SCORE  " + GameState.instance.highScore;
        }
    }

    void Update()
    {
        if (blinkText != null)
        {
            blinkText.enabled = Mathf.FloorToInt(Time.time * 2) % 2 == 0;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (resetGame && GameState.instance != null)
            {
                GameState.instance.ResetGame();
            }
            SceneManager.LoadScene(nextScene);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
