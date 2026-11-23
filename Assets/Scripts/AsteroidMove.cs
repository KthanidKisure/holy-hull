using UnityEngine;

// Drifting asteroid from Practical 4, with the collision particles from Practical 5.
// Big asteroids break into two smaller pieces when shot (one of the Practical 5 tasks).
public class AsteroidMove : MonoBehaviour
{
    public int rotz;
    public int mx, my;
    public GameObject[] pieces;
    public GameObject explosion;
    public int points = 10;

    private Rigidbody2D myRigid;
    private ParticleSystem myPart;

    // Awake, not Start: a freshly spawned piece can collide before its Start runs
    void Awake()
    {
        myRigid = this.GetComponent<Rigidbody2D>();
        myPart = this.GetComponent<ParticleSystem>();
    }

    void Start()
    {

        myRigid.AddForce(Vector3.up * my);
        myRigid.AddForce(Vector3.right * mx);
        myRigid.AddTorque(rotz);
    }

    public void Break()
    {
        Instantiate(explosion, this.transform.position, Quaternion.identity);
        GameState.instance.PlaySound(GameState.instance.explosionSound);
        GameState.instance.AdjustScore(points);

        // small asteroids have no pieces and just disappear
        for (int i = 0; i < pieces.Length; i++)
        {
            GameObject piece = Instantiate(pieces[i], this.transform.position, Quaternion.identity);
            AsteroidMove am = piece.GetComponent<AsteroidMove>();
            am.mx = Random.Range(-80, 80);
            am.my = Random.Range(-80, 80);
            am.rotz = Random.Range(-20, 20);
        }
        Destroy(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        myPart.Play();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        myPart.Stop();
    }
}
