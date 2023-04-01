using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    public class PlayerController : MonoBehaviour
    {
        public float movingSpeed;
        public float jumpForce;

        // Time after the player was last grounded to still allow jumping
        public float CoyoteTime;

        // Time since the last jump intention to allow when the player touched the ground
        public float JumpBuffer;

        private bool facingRight = false;
        [HideInInspector] public bool deathState = false;

        private bool isGrounded;
        public Transform groundCheck;

        private new Transform transform;
        private new Rigidbody2D rigidbody;
        private Animator animator;
        private GameManager gameManager;

        private Vector3 mouseDelta;
        private Vector3 smoothMouseDelta;
        private float lastJumpValue;
        private float minJumpInterval = 0.25f;
        private float nextJumpTime = float.NegativeInfinity;
        private Vector3 clickMousePosition;
        private Vector3 lastMousePosition;

        void Start()
        {
            transform = GetComponent<Transform>();
            rigidbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        private void FixedUpdate()
        {
            CheckGround();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                clickMousePosition = Input.mousePosition;
            }

            mouseDelta = Input.mousePosition - lastMousePosition;
            smoothMouseDelta = Vector3.Lerp(smoothMouseDelta, mouseDelta, 0.75f);

            if (!isGrounded || (Time.time > nextJumpTime) || (smoothMouseDelta.y < 0))
                lastJumpValue = 0;

            if (GetInputMove(out var moveValue))
            {
                moveValue *= movingSpeed;
                transform.position += transform.right * (moveValue * Time.deltaTime);
                animator.SetInteger("playerState", 1); // Turn on run animation
            }
            else
            {
                if (isGrounded) animator.SetInteger("playerState", 0); // Turn on idle animation
            }

            // if (GetInputJump(out var jumpValue) && isGrounded && (nextJumpTime < Time.time))
            if (GetInputJump(out var jumpValue) && isGrounded)
            {
                var deltaJump = Mathf.Max(jumpValue - lastJumpValue, 0);
                lastJumpValue = Mathf.Max(lastJumpValue, jumpValue);
                jumpValue = deltaJump * jumpForce;
                rigidbody.AddForce(transform.up * jumpValue, ForceMode2D.Impulse);
                nextJumpTime = Time.time + minJumpInterval;
            }

            if (!isGrounded) animator.SetInteger("playerState", 2); // Turn on jump animation

            if ((System.Math.Abs(moveValue) > 0) && (facingRight != (moveValue > 0)))
                Flip();

            lastMousePosition = Input.mousePosition;
        }

        private bool GetInputMove(out float value)
        {
            if (Input.GetMouseButton(0))
            {
                var range = Screen.width / 8f;
                var delta = (Input.mousePosition.x - clickMousePosition.x) / range;
                value = Mathf.Clamp(delta, -1, 1);
                return true;
            }

            value = Input.GetAxis("Horizontal");
            return Input.GetButton("Horizontal");
        }

        private bool GetInputJump(out float value)
        {
            if (Input.GetMouseButton(0))
            {
                var range = 16f;
                var delta = smoothMouseDelta.y / range;
                value = Mathf.Clamp(delta, 0, 1);
                return true;
            }

            value = 1;
            return Input.GetKeyDown(KeyCode.Space);
        }

        private void Flip()
        {
            facingRight = !facingRight;
            Vector3 scale = transform.localScale;
            scale.x = -scale.x;
            transform.localScale = scale;
        }

        private void CheckGround()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.transform.position, 0.2f);
            isGrounded = colliders.Length > 1;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.tag == "Enemy")
            {
                deathState = true; // Say to GameManager that player is dead
            }
            else
            {
                deathState = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag == "Coin")
            {
                gameManager.coinsCounter += 1;
                Destroy(other.gameObject);
            }
        }
    }
}