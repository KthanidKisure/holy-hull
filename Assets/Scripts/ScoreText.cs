using UnityEngine;
using UnityEngine.UI;

// Fills in {score} and {best} on the Victory and GameOver screens and saves the high score.
public class ScoreText : MonoBehaviour
{
    void Start()
    {
        Text t = this.GetComponent<Text>();
        GameState gs = GameState.instance;
        bool newBest = gs.SaveHighScore();
        string best = newBest ? "NEW HIGH SCORE!" : "BEST  " + gs.highScore;
        t.text = t.text.Replace("{score}", gs.score.ToString()).Replace("{best}", best);
    }
}
