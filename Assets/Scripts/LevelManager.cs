using UnityEngine;
using UnityEngine.UI;

// Runs one level: spawns asteroids and heretics (Practical 4), places the relic,
// opens the warp once the relic is taken, triggers holy malfunctions and updates the HUD.
public class LevelManager : MonoBehaviour
{
    public GameObject[] asteroidPrefabs;
    public GameObject hereticPrefab;
    public GameObject relicPrefab;
    public GameObject warp;

    public int howManyAsteroids = 6;
    public int howManyHeretics = 0;
    public float malfunctionEvery = 15f;
    public float malfunctionLength = 5f;
    public string levelName = "System I";

    // HUD
    public Image[] faithIcons;
    public Image[] relicIcons;
    public Sprite faithFull, faithEmpty, relicFull, relicEmpty;
    public Text scoreText;
    public GameObject alertPanel;
    public Text alertText;

    private MoveShip ship;
    private float nextMalfunction;
    private float alertEnds;

    void Start()
    {
        ship = GameObject.FindGameObjectWithTag("Player").GetComponent<MoveShip>();
        warp.SetActive(false);

        for (int i = 0; i < howManyAsteroids; i++)
        {
            GameObject prefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];
            GameObject tmpAsteroid = Instantiate(prefab, RandomSpotAwayFromShip(), Quaternion.Euler(0, 0, Random.Range(0, 360)));
            tmpAsteroid.transform.localScale *= Random.Range(0.8f, 1.3f);
            AsteroidMove am = tmpAsteroid.GetComponent<AsteroidMove>();
            am.mx = Random.Range(-60, 60);
            am.my = Random.Range(-60, 60);
            am.rotz = Random.Range(-12, 12);
        }

        for (int i = 0; i < howManyHeretics; i++)
        {
            Instantiate(hereticPrefab, RandomSpotAwayFromShip(), Quaternion.identity);
        }

        Instantiate(relicPrefab, RandomSpotAwayFromShip(), Quaternion.identity);

        nextMalfunction = Time.time + malfunctionEvery;
        ShowAlert(levelName + "\nFind the relic!", 3f);
    }

    void Update()
    {
        if (Time.time > nextMalfunction)
        {
            StartMalfunction();
        }
        if (Time.time > alertEnds && alertPanel.activeSelf)
        {
            EndMalfunction();
        }

        GameState gs = GameState.instance;
        for (int i = 0; i < faithIcons.Length; i++)
        {
            faithIcons[i].sprite = i < gs.faith ? faithFull : faithEmpty;
        }
        for (int i = 0; i < relicIcons.Length; i++)
        {
            relicIcons[i].sprite = i < gs.relics ? relicFull : relicEmpty;
        }
        scoreText.text = gs.score.ToString("00000");
    }

    public void OpenWarp()
    {
        warp.SetActive(true);
        EndMalfunction();
        ShowAlert("Relic secured!\nFly into the warp", 3f);
    }

    private void ShowAlert(string message, float seconds)
    {
        alertText.text = message;
        alertPanel.SetActive(true);
        alertEnds = Time.time + seconds;
    }

    private void StartMalfunction()
    {
        EndMalfunction();
        int pick = Random.Range(0, 3);
        if (pick == 0)
        {
            ship.controlsReversed = true;
            ShowAlert("Brother Navigator is steering backwards!\nCONTROLS REVERSED", malfunctionLength);
        }
        else if (pick == 1)
        {
            ship.cannonJammed = true;
            ShowAlert("The gunner is polishing the cannon!\nCANNON JAMMED", malfunctionLength);
        }
        else
        {
            ship.enginePraying = true;
            ShowAlert("The engineer stopped to pray!\nNO THRUST", malfunctionLength);
        }
        nextMalfunction = Time.time + malfunctionEvery + Random.Range(-3f, 3f);
    }

    private void EndMalfunction()
    {
        ship.controlsReversed = false;
        ship.cannonJammed = false;
        ship.enginePraying = false;
        alertPanel.SetActive(false);
    }

    private Vector3 RandomSpotAwayFromShip()
    {
        Vector3 spot;
        do
        {
            spot = new Vector3(Random.Range(-7.5f, 7.5f), Random.Range(-3.8f, 3.3f), 0f);
        } while (Vector3.Distance(spot, ship.transform.position) < 3f);
        return spot;
    }
}
