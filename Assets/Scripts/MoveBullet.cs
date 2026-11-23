using UnityEngine;

// Holy bolt fired by the ship. Based on MoveBullet from Practical 5.
public class MoveBullet : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 2f;

    private Rigidbody2D myRigid;

    void Start()
    {
        myRigid = this.GetComponent<Rigidbody2D>();
        myRigid.linearVelocity = this.transform.up * speed;
        // missiles disappear if they don't hit anything
        Destroy(this.gameObject, lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject gob = collision.gameObject;

        if (gob.tag == "Asteroid")
        {
            gob.GetComponent<AsteroidMove>().Break();
        }
        else if (gob.tag == "Heretic")
        {
            gob.GetComponent<HereticShip>().Hit();
        }
        Destroy(this.gameObject);
    }
}
