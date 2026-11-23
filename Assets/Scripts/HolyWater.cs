using UnityEngine;

// Dropped by heretics sometimes. Gives back one faith.
public class HolyWater : MonoBehaviour
{
    public float lifeTime = 8f;

    void Start()
    {
        Destroy(this.gameObject, lifeTime);
    }

    void Update()
    {
        this.transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 6f) * 0.1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            GameState.instance.AddFaith();
            Destroy(this.gameObject);
        }
    }
}
