using UnityEngine;

// Brother Astro on the menu screen. Same Idle/Run animator setup as the Animations practical.
public class AstroAnimation : MonoBehaviour
{
    public float speed = 2f;
    Animator anim;
    SpriteRenderer myRenderer;

    void Start()
    {
        anim = this.GetComponent<Animator>();
        myRenderer = this.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetKey("d"))
        {
            anim.SetBool("Running", true);
            myRenderer.flipX = false;
            this.transform.Translate(new Vector3(speed * Time.deltaTime, 0f, 0f));
        }
        else if (Input.GetKey("a"))
        {
            myRenderer.flipX = true;
            anim.SetBool("Running", true);
            this.transform.Translate(new Vector3(-speed * Time.deltaTime, 0f, 0f));
        }
        else
        {
            anim.SetBool("Running", false);
        }

        // keep him on screen
        Vector3 p = this.transform.position;
        p.x = Mathf.Clamp(p.x, -8f, 8f);
        this.transform.position = p;
    }
}
