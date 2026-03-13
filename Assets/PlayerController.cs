using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Min(0.1f)] public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Weapon weapon;

    private Vector2 moveDirection;
    private Vector2 mousePosition;
    private Camera mainCamera;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        if (Input.GetMouseButtonDown(0) && weapon != null)
        {
            weapon.Fire();
        }

        moveDirection = new Vector2(moveX, moveY).normalized;

        if (mainCamera != null)
        {
            mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        rb.velocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);

        Vector2 aimDirection = mousePosition - rb.position;
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = aimAngle;
    }
}
