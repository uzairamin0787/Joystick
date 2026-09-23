using UnityEngine;
using UnityEngine.InputSystem;

namespace MysticJungle
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class Week4Player : MonoBehaviour
    {
        public Joystick joystick;
        public ParticleSystem jumpVFX, collisionVFX;
        public float speed = 7, jumpImpulse = 6;
        Rigidbody body;
        Animator animator;
        bool grounded;
        float nextJump, nextDamage;
        public bool CanMove { get; set; }
        void Awake() { body = GetComponent<Rigidbody>(); animator = GetComponentInChildren<Animator>(); }
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) Jump();
            if (CanMove && transform.position.y < -3) Week4Game.Instance.Damage(true);
        }
        void FixedUpdate()
        {
            Vector2 input = joystick != null ? joystick.InputDirection : Vector2.zero;
            var k = Keyboard.current;
            if (CanMove && k != null)
            {
                Vector2 keys = new Vector2((k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0),
                    (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0));
                if (keys.sqrMagnitude > input.sqrMagnitude) input = keys;
            }
            if (!CanMove) input = Vector2.zero;
            input = Vector2.ClampMagnitude(input, 1);
            grounded = Physics.Raycast(transform.position + Vector3.up * .18f, Vector3.down, .3f, 1 << 6, QueryTriggerInteraction.Ignore);
            Vector3 move = new Vector3(input.x, 0, input.y);
            body.linearVelocity = new Vector3(move.x * speed, body.linearVelocity.y, move.z * speed);
            if (move.sqrMagnitude > .001f) body.MoveRotation(Quaternion.Slerp(body.rotation, Quaternion.LookRotation(move), 12 * Time.fixedDeltaTime));
            if (animator) animator.SetFloat("Speed", input.magnitude, .15f, Time.fixedDeltaTime);
        }
        public void Jump()
        {
            if (!CanMove || !grounded || Time.time < nextJump) return;
            nextJump = Time.time + .35f;
            body.linearVelocity = new Vector3(body.linearVelocity.x, jumpImpulse, body.linearVelocity.z);
            if (animator) animator.SetTrigger("Jump");
            if (jumpVFX) jumpVFX.Play();
        }
        public void Teleport(Vector3 position)
        {
            body.position = position; transform.position = position; body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero;
        }
        void OnCollisionEnter(Collision collision)
        {
            if (!CanMove || !collision.gameObject.CompareTag("Obstacle") || Time.time < nextDamage) return;
            nextDamage = Time.time + 1.5f;
            if (collisionVFX) { collisionVFX.transform.position = collision.GetContact(0).point; collisionVFX.Play(); }
            Week4Game.Instance.Damage(false);
        }
    }
}
