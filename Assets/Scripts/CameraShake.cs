using UnityEngine;

// Small screen shake when the ship gets hit or something big explodes.
public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;
    private Vector3 startPos;
    private float shakeUntil;
    private float strength;

    void Awake()
    {
        instance = this;
        startPos = this.transform.position;
    }

    public static void Shake(float time, float amount)
    {
        if (instance != null)
        {
            instance.shakeUntil = Time.time + time;
            instance.strength = amount;
        }
    }

    void Update()
    {
        if (Time.time < shakeUntil)
        {
            Vector2 offset = Random.insideUnitCircle * strength;
            this.transform.position = startPos + new Vector3(offset.x, offset.y, 0f);
        }
        else
        {
            this.transform.position = startPos;
        }
    }
}
