using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    public Bullet bulletPrefab;
    public Transform firePoint;      // empty child object
    public float fireCooldown = 0.2f;

    float nextFireTime;

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireCooldown;
            Shoot();
        }
    }

    void Shoot()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouseWorld - firePoint.position);
        dir.Normalize();

        Bullet b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        b.Fire(dir);
    }
}