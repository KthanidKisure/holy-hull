using UnityEngine;

// Heretic drone. Turns to face the player and pushes itself towards them with forces.
public class HereticShip : MonoBehaviour
{
    public float chaseForce = 2.5f;
    public float maxSpeed = 2.5f;

    private Rigidbody2D myRigid;
    private Transform player;

    void Start()
    {
        myRigid = this.GetComponent<Rigidbody2D>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
        }
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
}
