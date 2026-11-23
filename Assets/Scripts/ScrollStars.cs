using UnityEngine;

// Slides a star layer sideways and wraps it round, so space feels like it is moving.
public class ScrollStars : MonoBehaviour
{
    public float speed = 0.3f;
    public float width = 17.78f;

    void Update()
    {
        this.transform.Translate(Vector3.left * speed * Time.deltaTime);
        if (this.transform.position.x < -width)
        {
            this.transform.Translate(Vector3.right * width * 2f);
        }
    }
}
