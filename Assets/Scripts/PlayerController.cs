using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 8f;
    public float rotationSpeed = 10f;
    public Joystick joystick;

    [Header("Animation")]
    public float animationSpeedChange = 5f;
    public float animationStopSpeed = 0.8f;
    [Range(0f, 1f)]
    public float runThreshold = 0.5f;

    [Header("Jump")]
    public float jumpForce = 6f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    public ParticleSystem jumpParticles;
    public ParticleSystem collisionParticles;

    private float animationSpeed = 0f;
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
            Jump();
    }

    public void Jump()
    {
        if (!isGrounded)
            return;

        isGrounded = false;

        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // Play jump particle effect
        if (jumpParticles != null)
            jumpParticles.Play();

        if (animator != null)
            animator.SetTrigger("Jump");
    }

    private void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (joystick != null && joystick.InputDirection.magnitude > 0.01f)
        {
            horizontal = joystick.InputDirection.x;
            vertical = joystick.InputDirection.y;
        }

        Vector3 movement = new Vector3(horizontal, 0f, vertical);
        float input = Mathf.Clamp01(movement.magnitude);

        if (input > 0.01f)
        {
            movement.Normalize();

            // Walk or Run movement
            float speed = input < runThreshold ? walkSpeed : runSpeed;

            rb.MovePosition(
                rb.position + movement * speed * Time.fixedDeltaTime);

            // Rotation
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime));

            // Walk or Run animation
            float targetAnimation = input < runThreshold ? 0.5f : 1f;

            animationSpeed = Mathf.MoveTowards(
                animationSpeed,
                targetAnimation,
                animationSpeedChange * Time.fixedDeltaTime);
        }
        else
        {
            // Player stops immediately,
            // but animation goes Run → Walk → Idle.
            animationSpeed = Mathf.MoveTowards(
                animationSpeed,
                0f,
                animationStopSpeed * Time.fixedDeltaTime);
        }

        if (animator != null)
            animator.SetFloat("Speed", animationSpeed);

        // Ground check
        isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDistance + 0.1f,
            groundLayer);

        // Safety
        if (rb.linearVelocity.magnitude > 20f)
            rb.linearVelocity = rb.linearVelocity.normalized * 20f;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (collisionParticles != null)
            {
                collisionParticles.transform.position = collision.contacts[0].point;
                collisionParticles.Play();
            }
        }
    }
}