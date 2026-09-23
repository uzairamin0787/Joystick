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
        PhysicsMaterial movementMaterial;

        bool grounded;
        float nextJump;
        public bool CanMove { get; set; }
        void Awake()
        {
            body = GetComponent<Rigidbody>(); animator = GetComponentInChildren<Animator>();
            // Do not let friction pin the capsule against an obstacle during a jump.
            movementMaterial = new PhysicsMaterial("Week4 Player - No Friction")
            {
                staticFriction = 0f, dynamicFriction = 0f, bounciness = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };
            GetComponent<CapsuleCollider>().sharedMaterial = movementMaterial;
        }
        void OnDestroy() { if (movementMaterial) Destroy(movementMaterial); }
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) Jump();
            if (CanMove && (transform.position.y < -2f || !Week4Game.Instance.Route.IsOnPath(transform.position)))
                Week4Game.Instance.EndRun("You left the path");
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
            if (animator) animator.SetFloat("Speed", new Vector2(body.linearVelocity.x, body.linearVelocity.z).magnitude / Mathf.Max(speed, .1f), .15f, Time.fixedDeltaTime);
        }
        public void Jump()
        {
            if (!CanMove || !grounded || Time.time < nextJump) return;
            nextJump = Time.time + .35f;
            grounded = false;
            body.linearVelocity = new Vector3(body.linearVelocity.x, jumpImpulse, body.linearVelocity.z);
            if (animator) animator.SetTrigger("Jump");
            if (jumpVFX) jumpVFX.Play();
        }
        public void Teleport(Vector3 position)
        {
            body.position = position; transform.position = position; body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero;
            body.rotation = Quaternion.identity;
            grounded = false; nextJump = 0;
            var trail = GetComponent<TrailRenderer>(); if (trail) trail.Clear();

        }
        void OnCollisionEnter(Collision collision)
        {
            if (!CanMove || !collision.collider.GetComponentInParent<Week4Hazard>()) return;
            if (collisionVFX) { collisionVFX.transform.position = collision.GetContact(0).point; collisionVFX.Play(); }
            Week4Game.Instance.EndRun("You hit " + collision.collider.GetComponentInParent<Week4Hazard>().name);
        }
    }
}
