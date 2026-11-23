using UnityEngine;

// Cleans up explosion effects after a few seconds.
public class AutoDestroy : MonoBehaviour
{
    public float delay = 2f;

    void Start()
    {
        Destroy(this.gameObject, delay);
    }
}
