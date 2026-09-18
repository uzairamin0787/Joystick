using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float rotationSpeed = 10f;
    public Joystick joystick;

    public float acceleration = 8f;
    public float deceleration = 4f;

    private float currentSpeed = 0f;

    [Header("Jump")]
    public float jumpForce = 6f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    private bool isGrounded = true;

    private Rigidbody rb;
    private Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    public void Jump()
    {
        if (!isGrounded)
            return;

        isGrounded = false;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
    }

    private void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (joystick != null &&
            joystick.InputDirection.magnitude > 0.1f)
        {
            horizontal = joystick.InputDirection.x;
            vertical = joystick.InputDirection.y;
        }

        Vector3 movement = new Vector3(horizontal, 0f, vertical);

        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }

        float targetSpeed = movement.magnitude * moveSpeed;

        if (movement.magnitude > 0.1f)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                deceleration * Time.fixedDeltaTime
            );
        }

        if (movement.magnitude > 0.1f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(movement);

            Quaternion smoothRotation =
                Quaternion.Slerp(
                    rb.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                );

            rb.MoveRotation(smoothRotation);
        }

        rb.MovePosition(
            rb.position +
            movement.normalized * currentSpeed * Time.fixedDeltaTime
        );

        // Ground check via raycast — pure read, no physics side effects
        isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDistance + 0.1f,
            groundLayer
        );

        if (animator != null)
        {
            float animationSpeed = currentSpeed / moveSpeed;
            animator.SetFloat("Speed", animationSpeed);
        }

        // Safety clamp — prevents any physics glitch from launching the player
        float maxVelocity = 20f;
        if (rb.linearVelocity.magnitude > maxVelocity)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
        }
    }
}