using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Min(0.1f)] public float lifeSeconds = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeSeconds);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
