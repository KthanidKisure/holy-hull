using UnityEngine;
using UnityEngine.UI;

// Fills in {score} on the Victory and GameOver screens.
public class ScoreText : MonoBehaviour
{
    void Start()
    {
        Text t = this.GetComponent<Text>();
        int score = 0;
        if (GameState.instance != null)
        {
            score = GameState.instance.score;
        }
        t.text = t.text.Replace("{score}", score.ToString());
    }
}
