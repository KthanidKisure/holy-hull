using UnityEngine;
using UnityEngine.SceneManagement;

// Holds everything that has to survive a scene change: faith (lives), score, relics,
// volume settings and the high score. Kept alive with DontDestroyOnLoad (Practical 7).
public class GameState : MonoBehaviour
{
    public static GameState instance;

    public int startFaith = 3;
    public int relicsNeeded = 3;

    public int faith;
    public int score;
    public int relics;
    public int highScore;

    public float musicVolume = 0.35f;
    public float sfxVolume = 0.8f;

    public AudioClip warpSound;
    public AudioClip relicSound;
    public AudioClip hitSound;
    public AudioClip explosionSound;
    public AudioClip shootSound;
    public AudioClip pickupSound;
    public AudioClip music;

    private AudioSource sfxSource;
    private AudioSource musicSource;

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

        // saved settings from last time (PlayerPrefs)
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", musicVolume);
        sfxVolume = PlayerPrefs.GetFloat("SfxVolume", sfxVolume);
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        // first AudioSource plays effects, the second one loops the music
        AudioSource[] sources = this.GetComponents<AudioSource>();
        sfxSource = sources[0];
        musicSource = sources[1];
        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.Play();
        SetMusicVolume(musicVolume);
        SetSfxVolume(sfxVolume);
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

    public void AddFaith()
    {
        faith = Mathf.Min(faith + 1, startFaith);
        PlaySound(pickupSound);
    }

    public void CollectRelic()
    {
        relics += 1;
        score += 100;
        PlaySound(relicSound);
    }

    // returns true if this run beat the old high score
    public bool SaveHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = v;
        musicSource.volume = v;
        PlayerPrefs.SetFloat("MusicVolume", v);
    }

    public void SetSfxVolume(float v)
    {
        sfxVolume = v;
        sfxSource.volume = v;
        PlayerPrefs.SetFloat("SfxVolume", v);
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
