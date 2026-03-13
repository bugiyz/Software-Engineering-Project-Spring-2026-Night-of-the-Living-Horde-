using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    [Min(0.1f)] public float fireForce = 20f;

    public void Fire()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Weapon is missing bulletPrefab or firePoint.", this);
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D bulletBody = bullet.GetComponent<Rigidbody2D>();
        if (bulletBody == null)
        {
            Debug.LogWarning("Bullet prefab is missing a Rigidbody2D.", bullet);
            return;
        }

        bulletBody.AddForce(firePoint.up * fireForce, ForceMode2D.Impulse);
    }
}
