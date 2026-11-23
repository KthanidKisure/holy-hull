using UnityEngine;
using UnityEngine.SceneManagement;

// The warp out of each level. Based on the BlackHole trigger from Practical 7.
public class BlackHole : MonoBehaviour
{
    public string whereToGo;
    public float spinSpeed = 90f;

    private bool warping = false;

    void Update()
    {
        this.transform.Rotate(new Vector3(0, 0, spinSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject gob = collision.gameObject;
        Rigidbody2D colrig;

        if (gob.tag == "Asteroid")
        {
            colrig = gob.GetComponent<Rigidbody2D>();
            colrig.AddForce(this.gameObject.transform.up * 500);
            colrig.AddTorque(Random.Range(-10, 10));
        }

        if (gob.tag == "Player" && !warping)
        {
            warping = true;
            GameState.instance.PlaySound(GameState.instance.warpSound);
            SceneManager.LoadScene(whereToGo);
        }
    }
}
