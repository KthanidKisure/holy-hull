using UnityEngine;

// Heretic drone. Turns to face the player and pushes itself towards them with forces.
// The Arch-Heretic on level 3 is the same script with more health.
public class HereticShip : MonoBehaviour
{
    public float chaseForce = 2.5f;
    public float maxSpeed = 2.5f;
    public int health = 1;
    public int points = 25;
    public GameObject explosion;
    public GameObject holyWater;
    public float dropChance = 0.25f;

    private Rigidbody2D myRigid;
    private SpriteRenderer myRenderer;
    private Transform player;
    private float flashUntil;

    void Start()
    {
        myRigid = this.GetComponent<Rigidbody2D>();
        myRenderer = this.GetComponent<SpriteRenderer>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
        }
    }

    void Update()
    {
        // flash white for a moment after being hit
        myRenderer.color = Time.time < flashUntil ? new Color(1f, 1f, 1f, 0.4f) : Color.white;
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            return;
        }

        Vector2 toPlayer = (player.position - this.transform.position).normalized;
        this.transform.up = toPlayer;

        if (myRigid.linearVelocity.magnitude < maxSpeed)
        {
            myRigid.AddForce(toPlayer * chaseForce);
        }
    }

    public void Hit()
    {
        health -= 1;
        flashUntil = Time.time + 0.1f;
        if (health > 0)
        {
            GameState.instance.PlaySound(GameState.instance.hitSound);
            return;
        }

        Instantiate(explosion, this.transform.position, Quaternion.identity);
        GameState.instance.PlaySound(GameState.instance.explosionSound);
        GameState.instance.AdjustScore(points);
        if (Random.value < dropChance)
        {
            Instantiate(holyWater, this.transform.position, Quaternion.identity);
        }
        if (points > 25)
        {
            CameraShake.Shake(0.4f, 0.25f);
        }
        Destroy(this.gameObject);
    }
}
