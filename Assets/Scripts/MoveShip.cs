using UnityEngine;

// The Holy Ship. Thrust and rotation with forces (Practical 2), firing with a
// rate limit (Practical 5), plus the crew's "holy malfunctions" set by LevelManager.
public class MoveShip : MonoBehaviour
{
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode up = KeyCode.W;
    public KeyCode fire = KeyCode.Space;

    public GameObject bullet;
    public float moveSpeed = 6f;
    public float rotationSpeed = 180f;
    public float fireDelay = 0.3f;
    public float invincibleTime = 1.5f;

    // engine flame, two frames swapped while thrusting
    public SpriteRenderer flame;
    public Sprite[] flameFrames;

    // malfunctions, switched on and off by the LevelManager
    public bool controlsReversed = false;
    public bool cannonJammed = false;
    public bool enginePraying = false;

    private Rigidbody2D myRigid;
    private SpriteRenderer myRenderer;
    private float lastFired;
    private float lastHit = -10f;
    private bool thrusting;

    void Start()
    {
        myRigid = this.GetComponent<Rigidbody2D>();
        myRenderer = this.GetComponent<SpriteRenderer>();
        // start each level invincible for a moment so nothing can hit you on spawn
        lastHit = Time.time;
    }

    void Update()
    {
        if (PauseMenu.paused)
        {
            return;
        }

        float turn = rotationSpeed;
        if (controlsReversed)
        {
            turn = -rotationSpeed;
        }

        // arrow keys work too
        if (Input.GetKey(left) || Input.GetKey(KeyCode.LeftArrow))
        {
            this.transform.Rotate(new Vector3(0f, 0f, turn) * Time.deltaTime);
        }
        if (Input.GetKey(right) || Input.GetKey(KeyCode.RightArrow))
        {
            this.transform.Rotate(new Vector3(0f, 0f, -turn) * Time.deltaTime);
        }

        thrusting = (Input.GetKey(up) || Input.GetKey(KeyCode.UpArrow)) && !enginePraying;

        // space or left mouse button fires
        if ((Input.GetKey(fire) || Input.GetMouseButton(0)) && !cannonJammed)
        {
            if (Time.time > lastFired + fireDelay)
            {
                Instantiate(bullet, this.transform.position + this.transform.up * 0.95f, this.transform.rotation);
                GameState.instance.PlaySound(GameState.instance.shootSound);
                lastFired = Time.time;            }
        }

        flame.enabled = thrusting;
        if (thrusting)
        {
            flame.sprite = flameFrames[Mathf.FloorToInt(Time.time * 12) % flameFrames.Length];
        }

        // flash while invincible after a hit
        bool flashing = Time.time < lastHit + invincibleTime;
        myRenderer.enabled = !flashing || Mathf.FloorToInt(Time.time * 10) % 2 == 0;
    }

    void FixedUpdate()
    {
        if (thrusting)
        {
            myRigid.AddForce(this.transform.up * moveSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;
        if (tag == "Asteroid" || tag == "Heretic")
        {
            TakeHit();
        }
    }

    public void TakeHit()
    {
        if (Time.time < lastHit + invincibleTime)
        {
            return;
        }
        lastHit = Time.time;
        CameraShake.Shake(0.25f, 0.15f);
        GameState.instance.LoseFaith();
    }
}
