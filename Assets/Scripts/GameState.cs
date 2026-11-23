using UnityEngine;
using UnityEngine.SceneManagement;

// Holds everything that has to survive a scene change: faith (lives), score and relics.
// The object is called GameState and is kept alive with DontDestroyOnLoad (Practical 7).
public class GameState : MonoBehaviour
{
    public static GameState instance;

    public int startFaith = 3;
    public int relicsNeeded = 3;

    public int faith;
    public int score;
    public int relics;

    public AudioClip warpSound;
    public AudioClip relicSound;
    public AudioClip hitSound;
    public AudioClip explosionSound;
    public AudioClip shootSound;
    public AudioClip music;

    private AudioSource myAudio;

    void Awake()
    {
        // only one GameState is allowed, extra copies from other scenes delete themselves
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        // first AudioSource plays effects, the second one loops the music
        AudioSource[] sources = this.GetComponents<AudioSource>();
        myAudio = sources[0];
        if (sources.Length > 1 && music != null)
        {
            sources[1].clip = music;
            sources[1].loop = true;
            sources[1].volume = 0.35f;
            sources[1].Play();
        }
        ResetGame();
    }

    public void ResetGame()
    {
        faith = startFaith;
        score = 0;
        relics = 0;
    }

    public void AdjustScore(int s)
    {
        score += s;
    }

    public void LoseFaith()
    {
        faith -= 1;
        PlaySound(hitSound);
        if (faith <= 0)
        {
            SceneManager.LoadScene("GameOver");
        }
    }

    public void CollectRelic()
    {
        relics += 1;
        score += 100;
        PlaySound(relicSound);
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            myAudio.PlayOneShot(clip);
        }
    }
}
