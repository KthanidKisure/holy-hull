using UnityEngine;

// The sacred relic for the level. Picking it up opens the warp.
public class Relic : MonoBehaviour
{
    public float spinSpeed = 60f;
    private Vector3 startPos;

    void Start()
    {
        startPos = this.transform.position;
    }

    void Update()
    {
        // bob up and down so it is easy to spot
        this.transform.position = startPos + new Vector3(0f, Mathf.Sin(Time.time * 3f) * 0.15f, 0f);
        this.transform.Rotate(new Vector3(0f, 0f, Mathf.Sin(Time.time) * spinSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            GameState.instance.CollectRelic();
            GameObject.Find("LevelManager").GetComponent<LevelManager>().OpenWarp();
            Destroy(this.gameObject);
        }
    }
}
