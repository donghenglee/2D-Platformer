using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    public Animator animator;
    Scene currentScene;
    Controller2D controller;
    SpriteRenderer Sprite;

    Vector2 directionalInput;
    float moveSpeed = 6;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;

    public float maxJumpHeight = 4;
    public float minJumpHeight = .5f;
    public float timetoJumpApex = .4f;
    public float timeToAirJumpApex = .4f;
    public float airJumpHeight = 4;
    public int numberOfJumps = 2;

    float gravity;
    float airGravity;
    Vector3 velocity;
    float airJumpVelocity;
    float minJumpVelocity;
    float maxJumpVelocity;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;
    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;
    float timeToWallUnstick;

    public int playerHealth;
    public int playerMaxHealth = 3;
    public int numOfHearts;
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [HideInInspector]
    public int jumpsRemaining;
    float velocityXSmoothing;
    bool m_FacingRight = true;

    bool isAirJumping;
    bool isAirborne;
    bool wallSliding;
    int wallDirX;


    //AudioController audioController;




    void Start()
    {
        
        numOfHearts= playerMaxHealth;
        controller = GetComponent<Controller2D>();
        Sprite = GetComponent<SpriteRenderer>();
        //audioController = GetComponent<AudioController>();
        currentScene = SceneManager.GetActiveScene();
        

        playerHealth = playerMaxHealth;
        Physics2D.queriesStartInColliders = false;

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timetoJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timetoJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);

        

        //AudioController.instance.playSound("BGM");
    }

    private void Update()
    {
        CalculateVelocity();
        HandleWallSliding();
   
        controller.Move(velocity * Time.deltaTime, directionalInput);

        //detecting jumpsRemaining, isAirJumping, isAirborne
        if (controller.collsions.above || controller.collsions.below)
        {
            if (controller.collsions.slidingDownMaxSlope) {
                velocity.y += controller.collsions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else {
                velocity.y = 0;
            }

            if (controller.collsions.below) {
                jumpsRemaining = numberOfJumps - 1;
                isAirJumping = false;
                isAirborne = false;
            }
            else {
                isAirborne = true;
            }

        }

        //animations
        //run
        animator.SetFloat("xSpeed", Mathf.Abs(velocity.x));

        //jump
        if (controller.collsions.below) {
            animator.SetBool("isJumping", false);
        }
        else {
            animator.SetBool("isJumping", true);
        }

        //glide
        animator.SetBool("isSliding", wallSliding);


        //flipping sprite
        if (velocity.x > 0 && !m_FacingRight) {
            Flip();
        }
        else if (velocity.x < 0 && m_FacingRight) {
            Flip();
        }
        
        //player health
        if (playerHealth <= 0)
        {
            //AudioController.instance.playSound("Die");
            playerHealth = 3;
            SceneManager.LoadScene(currentScene.buildIndex);
        }
                
        for (int i = 0 ; i< hearts.Length; i++)
        {
            if (i<playerHealth) {
                hearts[i].sprite= fullHeart;
            }
            else {
                hearts[i].sprite= emptyHeart;
            }

            if (i<numOfHearts) {
                hearts[i].enabled=true;
            }
            else {
                hearts[i].enabled=true;
            }
        }
    }

    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }

    public void OnJumpInputDown()
    {
        if (wallSliding)
        {
            if (wallDirX == directionalInput.x)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;

            }
            else if (directionalInput.x == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;

            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;

            }
        }
        if (controller.collsions.below)
        {
            if (controller.collsions.slidingDownMaxSlope)
            {
                if (directionalInput.x != -Mathf.Sign(controller.collsions.slopeNormal.x))//not jumping against max slope
                {
                    velocity.y = maxJumpVelocity * controller.collsions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collsions.slopeNormal.x;
                }
            }
            else
            {
                if (directionalInput.y != -1)
                {
                    velocity.y = maxJumpVelocity;
                }
            }
           
        }
        if (!controller.collsions.below && jumpsRemaining > 0  & !controller.collsions.slidingDownMaxSlope)
        {
            handleAirJumping();
        }

    }

    public void OnJumpInputUp()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
        if (isAirJumping)
        {
            velocity.y += airGravity * Time.deltaTime;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }

    void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collsions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        if (Mathf.Abs(velocity.x) < 0.001)
        {
            velocity.x = 0;
        }
        velocity.y += gravity * Time.deltaTime;
    }

    void HandleWallSliding()
    {
        wallDirX = (controller.collsions.left) ? -1 : 1;
        wallSliding = false;
        if ((controller.collsions.left || controller.collsions.right) && !controller.collsions.below && velocity.y < 0)
        {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }
            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;
                if (directionalInput.x != wallDirX && directionalInput.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }

    }

    public void handleAirJumping()
    {
        isAirJumping = true;
        airGravity = -(2 * airJumpHeight) / Mathf.Pow(timeToAirJumpApex, 2);
        airJumpVelocity = Mathf.Abs(airGravity) * timeToAirJumpApex;
        velocity.y = airJumpVelocity;
        jumpsRemaining -= 1;
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

   
    
}



