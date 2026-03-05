using UnityEngine;

public class ZombieDamageOnTouch : MonoBehaviour
{
    public int damage = 10;
    public float damageCooldown = 1.0f;

    float nextDamageTime;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time < nextDamageTime) return;

        if (collision.collider.CompareTag("Player"))
        {
            PlayerHealth ph = collision.collider.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
                nextDamageTime = Time.time + damageCooldown;
            }
        }
    }
}
