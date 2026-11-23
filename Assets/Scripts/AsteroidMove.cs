using UnityEngine;

// Drifting asteroid from Practical 4, with the collision particles from Practical 5.
public class AsteroidMove : MonoBehaviour
{
    public int rotz;
    public int mx, my;

    private Rigidbody2D myRigid;
    private ParticleSystem myPart;

    void Start()
    {
        myRigid = this.GetComponent<Rigidbody2D>();
        myPart = this.GetComponent<ParticleSystem>();

        myRigid.AddForce(Vector3.up * my);
        myRigid.AddForce(Vector3.right * mx);
        myRigid.AddTorque(rotz);
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
