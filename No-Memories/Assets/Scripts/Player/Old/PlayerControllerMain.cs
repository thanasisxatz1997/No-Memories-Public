using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerMain : MonoBehaviour
{

    private float movementInputDirection;

    public int amountOfJumps=1;

    public float movementSpeed=10.0f;
    public float jumpForce = 16.0f;
    public float groundCheckRadius;

    private int amountOfJumpsLeft;

    private bool isFacingRight=true;
    private bool isWalking;
    private bool isGrounded;
    private bool canJump;

    private Rigidbody2D rb;
    private Animator anim;

    public LayerMask whatIsGround;

    public Transform groundCheck;

    void Start()// Start is called before the first frame update
    {
        rb = GetComponent<Rigidbody2D>(); //store a reference to the rigid body on the player 
        anim = GetComponent<Animator>();
        amountOfJumpsLeft = amountOfJumps;
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();//Checks for input every frame
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
    }
    private void FixedUpdate()
    {
        ApplyMovement();//Calls Apply movement function to apply movement
        CheckSurroundings();//Calls checkSurroundings
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position,groundCheckRadius,whatIsGround);
    }

    private void CheckInput() // this function will get called to check for any kind of input we might expect from the player
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal"); //horizontal is referenced to the A and D keys by default Raw makes it always return 0 or 1, without raw it would return a float between 0 and 1 the longer you hold the higher the number
        if(Input.GetButtonDown("Jump"))
        {
            Jump();
        }
    }

    private void CheckIfCanJump()
    {
        if(isGrounded && rb.velocity.y<=0)
        {
            amountOfJumpsLeft = amountOfJumps;
        }
        if(amountOfJumpsLeft<=0)
        {
            canJump = false;
        }
        else
        {
            canJump = true;
        }
    }

    private void Jump()
    {
        if(canJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amountOfJumpsLeft--;
        }
    }

    private void CheckMovementDirection()
    {
        if(isFacingRight && movementInputDirection<0)
        {
            Flip();
        }
        else if(!isFacingRight && movementInputDirection>0)
        {
            Flip();
        }

        if(rb.velocity.x!=0)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    private void UpdateAnimations()
    {
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
    }

    private void ApplyMovement()
    {
        rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
        
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
