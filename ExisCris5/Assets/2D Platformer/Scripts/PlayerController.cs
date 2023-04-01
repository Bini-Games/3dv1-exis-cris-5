using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    public class PlayerController : MonoBehaviour
    {
        public float movingSpeed;
        public float jumpForce;

        public float GroundDetectionRadius = 0.2f;
        public float NearGroundDetectionRadius = 0.2f;

        public float CoyoteTime;
        public float JumpBuffer;
        public float JumpResetTime = 0.25f;
        public float JumpRetractTime = 0.25f;
        public float JumpDecay = 1f;

        public int SpriteDirection = 1;

        public float InputRangeX = 1f / 8f;
        public float InputRangeY = 16;

        [HideInInspector] public bool deathState = false;

        private bool isGrounded;
        private bool isNearGround;
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

        private JumpController jumpController = new JumpController();

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
            else
            {
                var absoluteRangeX = Screen.width * InputRangeX;
                var delta = clickMousePosition.x - Input.mousePosition.x;
                if (Mathf.Abs(delta) > absoluteRangeX)
                {
                    clickMousePosition.x = Input.mousePosition.x + Mathf.Sign(delta) * absoluteRangeX;
                }
            }

            if (Input.GetMouseButton(0))
            {
                mouseDelta = Input.mousePosition - lastMousePosition;
                smoothMouseDelta = Vector3.Lerp(smoothMouseDelta, mouseDelta, 0.75f);
            }
            else
            {
                mouseDelta = default;
                smoothMouseDelta = default;
            }

            {
                var range = 16f;
                var delta = smoothMouseDelta.y / range;
                jumpController.CoyoteTime = CoyoteTime;
                jumpController.JumpBuffer = JumpBuffer;
                jumpController.JumpResetTime = JumpResetTime;
                jumpController.JumpRetractTime = JumpRetractTime;
                jumpController.JumpDecay = JumpDecay;
                jumpController.Update(isNearGround, delta);
            }

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
            // if (GetInputJump(out var jumpValue) && isGrounded)
            // {
            //     var deltaJump = Mathf.Max(jumpValue - lastJumpValue, 0);
            //     lastJumpValue = Mathf.Max(lastJumpValue, jumpValue);
            //     jumpValue = deltaJump * jumpForce;
            //     rigidbody.AddForce(transform.up * jumpValue, ForceMode2D.Impulse);
            //     nextJumpTime = Time.time + minJumpInterval;
            // }
            if (jumpController.RelativeImpulse > 0)
            {
                var velocity = rigidbody.velocity;
                velocity.y = Mathf.Max(velocity.y, 0);
                rigidbody.velocity = velocity;

                var jumpValue = jumpController.RelativeImpulse * jumpForce;
                rigidbody.AddForce(transform.up * jumpValue, ForceMode2D.Impulse);
                nextJumpTime = Time.time + minJumpInterval;
            }

            if (!isGrounded)
            {
                var isUp = rigidbody.velocity.y > 0;
                animator.SetInteger("playerState", isUp ? 2 : 3); // Turn on jump animation
            }

            if ((System.Math.Abs(moveValue) > 0))
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (SpriteDirection == (int) Mathf.Sign(moveValue) ? 1 : -1);
                transform.localScale = scale;
            }

            lastMousePosition = Input.mousePosition;
        }

        private void OnGUI()
        {
            GUI.Button(new Rect(0, 0, 120, 20), $"JI={jumpController.jumpIntention}");

            if (Input.GetMouseButton(0))
            {
                var rect = new Rect(0, 0, Screen.width * InputRangeX * 2, InputRangeY * 2);
                rect.center = new Vector2(clickMousePosition.x, Screen.height - clickMousePosition.y);
                GUI.Button(rect, "+");
            }
        }

        private bool GetInputMove(out float value)
        {
            if (Input.GetMouseButton(0))
            {
                var range = Screen.width * InputRangeX;
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
                var delta = smoothMouseDelta.y / InputRangeY;
                value = Mathf.Clamp(delta, 0, 1);
                return true;
            }

            value = 1;
            return Input.GetKeyDown(KeyCode.Space);
        }

        private Collider2D[] physicsCastResults = new Collider2D[100];

        private void CheckGround()
        {
            var center = groundCheck.transform.position;

            var size = Physics2D.OverlapCircleNonAlloc(center, GroundDetectionRadius, physicsCastResults);
            isGrounded = size > 1;

            var velocity = rigidbody.velocity;
            var radius = GroundDetectionRadius + Mathf.Max(-velocity.y * Time.deltaTime * NearGroundDetectionRadius, 0);
            size = Physics2D.OverlapCircleNonAlloc(center, radius, physicsCastResults);
            isNearGround = size > 1;
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

        private class JumpController
        {
            // Time after the player was last grounded to still allow jumping
            public float CoyoteTime;

            // Time since the last jump intention to allow when the player touched the ground
            public float JumpBuffer;

            public float JumpResetTime = 0.25f;
            public float JumpRetractTime = 0.25f;

            public float JumpDecay = 1f;

            public float RelativeImpulse;

            private float lastJumpIntention;
            public float jumpIntention;
            private float jumpEndTime = float.NegativeInfinity;
            private float retractingTime = float.NegativeInfinity;

            private float lastGroundedTime = float.NegativeInfinity;

            public void Update(bool isGrounded, float relativeAcceleration)
            {
                var time = Time.time;

                if (isGrounded)
                    lastGroundedTime = time;

                // if (time > retractingTime + JumpRetractTime)
                // {
                //     jumpIntention = Mathf.Clamp01(jumpIntention + Mathf.Clamp01(relativeAcceleration));
                //
                //     if (jumpIntention - lastJumpIntention > 0.01f)
                //     {
                //         jumpEndTime = time;
                //     }
                //     else if (time > jumpEndTime + JumpResetTime)
                //     {
                //         retractingTime = time;
                //     }
                // }
                // else
                // {
                //     jumpIntention = 0;
                //     lastJumpIntention = 0;
                // }
                var jumpDecay = JumpDecay * Time.deltaTime;
                // jumpIntention = Mathf.Clamp01(jumpIntention + Mathf.Clamp01(relativeAcceleration) - jumpDecay);
                jumpIntention = Mathf.Clamp01(jumpIntention + relativeAcceleration - jumpDecay);
                lastJumpIntention = Mathf.Min(lastJumpIntention, jumpIntention);

                if (time <= lastGroundedTime + CoyoteTime)
                {
                    RelativeImpulse = jumpIntention - lastJumpIntention;
                }
                else
                {
                    RelativeImpulse = 0;
                }

                lastJumpIntention = jumpIntention;
            }
        }
    }
}