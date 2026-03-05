using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 2f;
    public int damage = 10;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Fire(Vector2 direction)
    {
        rb.velocity = direction.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Hit zombie?
        ZombieHealth zh = other.GetComponent<ZombieHealth>();
        if (zh != null)
        {
            zh.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Hit wall/obstacle?
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
{
    Destroy(gameObject);
}
    }
}