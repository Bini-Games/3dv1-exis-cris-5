using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Platformer
{
    public class PlayerController : MonoBehaviour
    {
        public AudioClip JumpSound;
        public AudioClip BlasterSound;
        public AudioClip DeathSound;
        
        public Joystick joystick;
        public float JoystickJumpUpper = 0.5f;
        public float JoystickJumpLower = 0.4f;

        public float movingSpeed;
        public float jumpForce;
        public float MovementDeadZone = 0.25f;

        public float NearGroundDetectionRadius = 0.2f;
        public float ArtifactCheckRadius = 1f;

        public float CoyoteTime;
        public float JumpDecay = 1f;
        public int JumpLevels = 4;

        public int SpriteDirection = 1;

        public LayerMask GroundMask = -1;
        public LayerMask ArtifactMask;

        public float InputRangeX = 1f / 8f;
        public float InputRangeY = 1f / 8f;

        public CapsuleCollider2D GroundDetector;
        public CapsuleCollider2D NearGroundDetector;

        public GameObject ProjectileTemplate;
        public float ShootTime = 0.125f;
        public float ProjectileSpeed = 1;
        
        [HideInInspector] public bool deathState;

        private bool isGrounded;
        private bool isNearGround;

        private new Transform transform;
        private new Rigidbody2D rigidbody;
        private Animator animator;
        private GameManager gameManager;

        private Vector3 mouseDelta;
        private Vector3 smoothMouseDelta;
        private float clickMouseTime = float.NegativeInfinity;
        private Vector3 clickMousePosition;
        private Vector3 lastMousePosition;

        private bool wasJumpJoystick;

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
            CheckArtifact();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                clickMousePosition = Input.mousePosition;
                clickMouseTime = Time.time;
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

            if (!joystick) {
                var delta = smoothMouseDelta.y / (InputRangeY * Screen.height);
                jumpController.CoyoteTime = CoyoteTime;
                jumpController.JumpDecay = JumpDecay;
                jumpController.JumpLevels = JumpLevels;
                jumpController.Update(isNearGround, delta);
            }

            if (GetInputMove(out var moveValue) && (Mathf.Abs(moveValue) > MovementDeadZone))
            {
                moveValue *= movingSpeed;
                transform.position += transform.right * (moveValue * Time.deltaTime);
                animator.SetInteger("playerState", 1); // Turn on run animation
            }
            else
            {
                if (isGrounded) animator.SetInteger("playerState", 0); // Turn on idle animation
            }

            if (GetInputJump(out var jumpValue) && isGrounded)
            {
                SoundPlayer.Instance.Play(JumpSound);
                jumpValue *= jumpForce;
                rigidbody.AddForce(transform.up * jumpValue, ForceMode2D.Impulse);
            }
            else if (jumpController.RelativeImpulse > 0)
            {
                var velocity = rigidbody.velocity;
                velocity.y = Mathf.Max(velocity.y, 0);
                rigidbody.velocity = velocity;

                var impulse = jumpController.RelativeImpulse * jumpForce;
                rigidbody.AddForce(transform.up * impulse, ForceMode2D.Impulse);
            }

            if (!isGrounded)
            {
                var isUp = rigidbody.velocity.y > 0;
                animator.SetInteger("playerState", isUp ? 2 : 3); // Turn on jump animation
            }

            if ((System.Math.Abs(moveValue) > 0))
            {
                AlignSprite(moveValue);
            }

            // if (Input.GetMouseButtonUp(0) && (Time.time < clickMouseTime + ShootTime))
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                var camera = Camera.main;
                var targetPosition = camera.ScreenToWorldPoint(Input.mousePosition);
                var sourcePosition = ProjectileTemplate.transform.position;
                var deltaVector = (Vector2)(targetPosition - sourcePosition);
                AlignSprite(deltaVector.x);
                
                var projectile = Instantiate(ProjectileTemplate);
                projectile.SetActive(true);
                projectile.transform.position = sourcePosition;
                var projectileRigidbody = projectile.GetComponent<Rigidbody2D>();
                projectileRigidbody.velocity = deltaVector.normalized * ProjectileSpeed;
                
                SoundPlayer.Instance.Play(BlasterSound);
            }
            
            lastMousePosition = Input.mousePosition;
        }

        private void AlignSprite(float moveValue)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (SpriteDirection == (int) Mathf.Sign(moveValue) ? 1 : -1);
            transform.localScale = scale;
        }

        // private void OnGUI()
        // {
        //     GUI.Button(new Rect(0, 0, 120, 20), $"JI={jumpController.jumpIntention}");
        //
        //     if (Input.GetMouseButton(0))
        //     {
        //         var rect = new Rect(0, 0, Screen.width * InputRangeX * 2, InputRangeY * 2);
        //         rect.center = new Vector2(clickMousePosition.x, Screen.height - clickMousePosition.y);
        //         GUI.Button(rect, "+");
        //     }
        // }

        private bool GetInputMove(out float value)
        {
            if (!joystick)
            {
                if (Input.GetMouseButton(0))
                {
                    var range = Screen.width * InputRangeX;
                    var delta = (Input.mousePosition.x - clickMousePosition.x) / range;
                    value = Mathf.Clamp(delta, -1, 1);
                    return true;
                }
            }

            if (joystick && joystick.isActiveAndEnabled)
            {
                if (joystick.Horizontal != 0)
                {
                    value = joystick.Horizontal;
                    return true;
                }
            }

            value = Input.GetAxis("Horizontal");
            return Input.GetButton("Horizontal");
        }

        private bool GetInputJump(out float value)
        {
            // if (Input.GetMouseButton(0))
            // {
            //     var delta = smoothMouseDelta.y / InputRangeY;
            //     value = Mathf.Clamp(delta, 0, 1);
            //     return true;
            // }

            if (joystick && joystick.isActiveAndEnabled)
            {
                var isJumpJoystickNow = false;

                if (wasJumpJoystick)
                {
                    if (joystick.Vertical < JoystickJumpLower)
                    {
                        wasJumpJoystick = false;
                    }
                }
                else
                {
                    isJumpJoystickNow = joystick.Vertical > JoystickJumpUpper;
                    wasJumpJoystick = isJumpJoystickNow;
                }
                
                if (isJumpJoystickNow)
                {
                    value = 1;
                    return true;
                }
            }

            value = 1;
            return Input.GetKeyDown(KeyCode.Space);
        }

        private void CheckGround()
        {
            isGrounded = GroundDetector.IsTouchingLayers(GroundMask);

            var nearSize = GroundDetector.size;
            nearSize.y += Mathf.Max(-rigidbody.velocity.y * Time.deltaTime * NearGroundDetectionRadius, 0);
            NearGroundDetector.size = nearSize;
            isNearGround = NearGroundDetector.IsTouchingLayers(GroundMask);
        }

        private void CheckArtifact()
        {
            var center = transform.position;
            var collider = Physics2D.OverlapCircle(center, ArtifactCheckRadius, ArtifactMask);

            if (collider)
            {
                var spriteRenderer = collider.GetComponent<SpriteRenderer>();
                gameManager.AddArtifact(spriteRenderer.sprite);
                Destroy(collider.gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.tag == "Enemy")
            {
                deathState = true;
                SoundPlayer.Instance.Play(DeathSound);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag == "Coin")
            {
                gameManager.coinsCounter += 1;
                Destroy(other.gameObject);
            }
            else if (other.gameObject.tag == "Enemy")
            {
                deathState = true;
                SoundPlayer.Instance.Play(DeathSound);
            }
        }

        private class JumpController
        {
            // Time after the player was last grounded to still allow jumping
            public float CoyoteTime;
            public float JumpDecay = 1f;
            public int JumpLevels = 4;

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

                jumpIntention = Mathf.Clamp01(jumpIntention + relativeAcceleration - JumpDecay * Time.deltaTime);

                var snappedIntention = Mathf.FloorToInt(jumpIntention * (JumpLevels + 1)) / (float) JumpLevels;
                snappedIntention = Mathf.Clamp01(snappedIntention);
                lastJumpIntention = Mathf.Min(lastJumpIntention, snappedIntention);

                if (time <= lastGroundedTime + CoyoteTime)
                {
                    RelativeImpulse = snappedIntention - lastJumpIntention;
                }
                else
                {
                    RelativeImpulse = 0;
                }

                lastJumpIntention = snappedIntention;
            }
        }
    }
}