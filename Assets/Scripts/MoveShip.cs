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
    public float fireDelay = 0.35f;
    public float invincibleTime = 1.5f;

    // malfunctions, switched on and off by the LevelManager
    public bool controlsReversed = false;
    public bool cannonJammed = false;
    public bool enginePraying = false;

    private Rigidbody2D myRigid;
    private ParticleSystem myPart;
    private SpriteRenderer myRenderer;
    private float lastFired;
    private float lastHit = -10f;

    void Start()
    {
        myRigid = this.GetComponent<Rigidbody2D>();
        myPart = this.GetComponentInChildren<ParticleSystem>();
        myRenderer = this.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float turn = rotationSpeed;
        if (controlsReversed)
        {
            turn = -rotationSpeed;
        }

        if (Input.GetKey(left))
        {
            this.transform.Rotate(new Vector3(0f, 0f, turn) * Time.deltaTime);
        }
        if (Input.GetKey(right))
        {
            this.transform.Rotate(new Vector3(0f, 0f, -turn) * Time.deltaTime);
        }

        if (Input.GetKey(fire) && !cannonJammed)
        {
            if (Time.time > lastFired + fireDelay)
            {
                Instantiate(bullet, this.transform.position + this.transform.up * 0.85f, this.transform.rotation);
                GameState.instance.PlaySound(GameState.instance.shootSound);
                lastFired = Time.time;
            }
        }

        // flash while invincible after a hit
        bool flashing = Time.time < lastHit + invincibleTime;
        myRenderer.enabled = !flashing || Mathf.FloorToInt(Time.time * 10) % 2 == 0;
    }

    void FixedUpdate()
    {
        if (Input.GetKey(up) && !enginePraying)
        {
            myRigid.AddForce(this.transform.up * moveSpeed);
            if (!myPart.isPlaying)
            {
                myPart.Play();
            }
        }
        else
        {
            myPart.Stop();
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
        GameState.instance.LoseFaith();
    }
}
