using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : RayCastController
{

    public float maxSlopeAngle = 80;
    public float playerInvisibilityTime = 1f;

    public CollisionInfo collsions;
    [HideInInspector]
    public Vector2 playerInput;
    public bool isInvisible = false;
    float currentTime;
    float elapsedTime;
    Player player;
    public Rigidbody2D playerRigidBody;
    public float knockBackStregth;
    


    public override void Start()
    {
        base.Start();
        collsions.faceDir = 1;
        player = FindObjectOfType<Player>();
        playerRigidBody = player.GetComponent<Rigidbody2D>();
    }

    public void Move(Vector2 moveAmount, bool standingOnPlatform)
    {
        Move(moveAmount, Vector2.zero, standingOnPlatform);
    }

    public void Move(Vector2 moveAmount, Vector2 input, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();
        collsions.Reset();
        collsions.moveAmountOld = moveAmount;
        playerInput = input;

        if (moveAmount.y < 0)
        {
            Descendslope(ref moveAmount);
        }
        if (moveAmount.x != 0)
        {
            collsions.faceDir = (int)Mathf.Sign(moveAmount.x);
        }
        
        HorizontalCollisions(ref moveAmount);

        if (moveAmount.y != 0)
        {
            VerticalCollisions(ref moveAmount);
        }

        transform.Translate(moveAmount);

        if (standingOnPlatform == true)
        {
            collsions.below = true;
        }

    }

    void HorizontalCollisions(ref Vector2 moveAmount)
    {
        float directionX = collsions.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + skinwidth;

        if (Mathf.Abs(moveAmount.x) < skinwidth)
        {
            rayLength = 2 * skinwidth;
        }
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomleft : raycastOrigins.bottomright;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

            if (hit.distance == 0)
            {
                continue;
            }

            if (hit && hit.distance != 0)
            {
                //isInvisible not working
                if (hit.collider.tag == "Hostile" && isInvisible == false)
                {
                    KnockBackHit(hit);
                    isInvisible = true;
                    player.playerHealth -= 1;
                    currentTime = Time.time;

                }
                else if (hit.collider.tag == "Hostile" && isInvisible == true)
                {
                    elapsedTime = Time.time;
                    if (elapsedTime - currentTime > playerInvisibilityTime)
                    {
                        isInvisible = false;
                    }
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (collsions.descendingSlope)
                    {
                        collsions.descendingSlope = false;
                        moveAmount = collsions.moveAmountOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collsions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinwidth;
                        moveAmount.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
                    moveAmount.x += distanceToSlopeStart * directionX;
                }

                if (!collsions.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    moveAmount.x = Mathf.Min(Mathf.Abs(moveAmount.x), (hit.distance - skinwidth)) * directionX;
                    rayLength = Mathf.Min(Mathf.Abs(moveAmount.x) + skinwidth, hit.distance);

                    if (collsions.climbingSlope)
                    {
                        moveAmount.y = Mathf.Tan(collsions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
                    }
                    collsions.left = directionX == -1;
                    collsions.right = directionX == 1;
                }

            }
        }
    }

    void VerticalCollisions(ref Vector2 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + skinwidth;
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomleft : raycastOrigins.topleft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

            if (hit && hit.distance != 0) 
            {

                if (hit.collider.tag == "Hostile" && isInvisible == false)
                {
                    KnockBackHit(hit);
                    isInvisible = true;
                    currentTime = Time.time;
                    player.playerHealth -= 1;

                }
                else if (hit.collider.tag == "Hostile" && isInvisible == true)
                {
                    elapsedTime = Time.time;
                    if (elapsedTime - currentTime > playerInvisibilityTime)
                    {
                        isInvisible = false;
                    }
                }

                if (hit.collider.tag == "Through")
                {
                    if (hit.collider == collsions.fallingTroughPlatform)
                    {
                        continue;
                    }
                    if (directionY == 1 || hit.distance == 0)
                    {
                        continue;
                    }
                    if (playerInput.y == -1)
                    {
                        collsions.fallingTroughPlatform = hit.collider;
                        continue;
                    }
                }
                collsions.fallingTroughPlatform = null;
                moveAmount.y = (hit.distance - skinwidth) * directionY;
                rayLength = hit.distance;

                if (collsions.climbingSlope)
                {
                    moveAmount.x = moveAmount.y / Mathf.Tan(collsions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
                }
                collsions.below = directionY == -1;
                collsions.above = directionY == 1;


            }
        }

        if (collsions.climbingSlope)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            rayLength = Mathf.Abs(moveAmount.x) + skinwidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomleft : raycastOrigins.bottomright) + Vector2.up * moveAmount.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit && hit.distance != 0)
            {
                float slopAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopAngle != collsions.slopeAngle)
                {
                    moveAmount.x = Mathf.Min(Mathf.Abs(moveAmount.x), (hit.distance - skinwidth)) * directionX;
                    collsions.slopeAngle = slopAngle;
                    collsions.slopeNormal = hit.normal;
                }
            }
        }
    }

    void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal)
    {
        float moveDistance = Mathf.Abs(moveAmount.x);
        float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (moveAmount.y <= climbmoveAmountY)
        {
            moveAmount.y = climbmoveAmountY;
            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
            collsions.below = true;
            collsions.climbingSlope = true;
            collsions.slopeAngle = slopeAngle;
            collsions.slopeNormal = slopeNormal;
        }
    }

    void Descendslope(ref Vector2 moveAmount)
    {
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomleft, Vector2.down, Mathf.Abs(moveAmount.y) + skinwidth, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomright, Vector2.down, Mathf.Abs(moveAmount.y) + skinwidth, collisionMask);
        if (maxSlopeHitLeft ^ maxSlopeHitRight)
        {
            SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
            SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
        }

        if (!collsions.slidingDownMaxSlope)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomright : raycastOrigins.bottomleft);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - skinwidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                        {
                            float moveDistance = Mathf.Abs(moveAmount.x);
                            float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                            moveAmount.y -= descendmoveAmountY;

                            collsions.slopeAngle = slopeAngle;
                            collsions.descendingSlope = true;
                            collsions.below = true;
                            collsions.slopeNormal = hit.normal;
                        }
                    }
                }
            }
        }
    }

    void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount)
    {
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle)
            {
                moveAmount.x = hit.normal.x * ((Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad));

                collsions.slopeAngle = slopeAngle;
                collsions.slidingDownMaxSlope = true;
                collsions.slopeNormal = hit.normal;
            }
        }
    }

    public void KnockBackHit(RaycastHit2D hit)
    {
        Vector3 direction = hit.normal.normalized;
        player.transform.position += direction * knockBackStregth;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hostile") && isInvisible == false)
        {
            player.playerHealth -= 1;
        }
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public bool slidingDownMaxSlope;

        public float slopeAngle, slopeAngleOld;
        public Vector2 slopeNormal;
        public Vector2 moveAmountOld;
        public int faceDir;
        public Collider2D fallingTroughPlatform;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slidingDownMaxSlope = false;

            slopeAngleOld = slopeAngle;
            slopeNormal = Vector2.zero;
            slopeAngle = 0;

        }
    }

}

