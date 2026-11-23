using UnityEngine;

// Plays a list of sprites in order. Used for the explosion, which deletes itself at the end.
public class SpriteAnimator : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 12f;
    public bool destroyAtEnd = true;

    private SpriteRenderer myRenderer;
    private float startTime;

    void Start()
    {
        myRenderer = this.GetComponent<SpriteRenderer>();
        startTime = Time.time;
    }

    void Update()
    {
        int frame = Mathf.FloorToInt((Time.time - startTime) * fps);
        if (frame >= frames.Length)
        {
            if (destroyAtEnd)
            {
                Destroy(this.gameObject);
                return;
            }
            frame = frame % frames.Length;
        }
        myRenderer.sprite = frames[frame];
    }
}
