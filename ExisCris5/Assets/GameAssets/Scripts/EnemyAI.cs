using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    public class EnemyAI : DestructableEntity
    {
        public float moveSpeed = 1f;
        public LayerMask ground;

        public Collider2D GroundDetector;
        public Collider2D WallDetector;

        private Rigidbody2D rigidbody;

        void Start()
        {
            rigidbody = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            rigidbody.velocity = new Vector2(moveSpeed, rigidbody.velocity.y);
        }

        void FixedUpdate()
        {
            if (!GroundDetector.IsTouchingLayers(ground) || WallDetector.IsTouchingLayers(ground))
            {
                Flip();
            }
        }

        private void Flip()
        {
            var scale = transform.localScale;
            scale.x = -scale.x;
            transform.localScale = scale;
            moveSpeed *= -1;
        }
    }
}