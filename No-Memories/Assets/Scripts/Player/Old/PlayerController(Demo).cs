using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerDemo : MonoBehaviour
{
    private int amountOfJumpsLeft;//how many jumps we have left (while in the air)
    private int facingDirection=1;//direction we are facing(-1 will be left, 1 will be right)

    private float movementInputDirection; //We want to check if the player is giving any input to the character, here we store what direction the player is trying to move in
    public float movementSpeed = 10.0f;
    public float jumpForce = 16.0f;// In order for the character to jump we are gonna have to set the velocity of the y axis to something
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float wallSlideSpeed;
    public float movementForceInAir;//This will hold the force that is applied to the character as its attempting to move in the air
    public float airDragMultiplier=0.95f;//here we create a variable to slow down the character when he is in the air and we stop giving input(so if in the air and we stop pressing A or D the character slows down by the value of the variable)
    public float variableJumpHeightMultiplier=0.5f;//With this variable we multiply the jump height(if we want our character to jump higher for some condition(ex holding space bar longer))
    public float wallHopForce;// we define a float that will store the force that we wanna Hop off of the wall with
    public float wallJumpForce;// we define a float that will store the force that we wanna Jump off of the wall with

    public Vector2 wallHopDirection;//its going to store a vector that is used to determine the direction that wich we jump off of the walls(so this allows you to when you jump off of the walls if you want it to be a steeper or a lower angle)
    public Vector2 wallJumpDirection;//its going to store a vector that is used to determine the direction that wich we jump off of the walls(so this allows you to when you jump off of the walls if you want it to be a steeper or a lower angle)
    public int amountOfJumps = 1;//how many times we wanna be able to jump(while in the air)

    public Transform groundCheck;
    public Transform wallCheck;

    public LayerMask whatIsGround;
    private Rigidbody2D rb;
    private Animator anim; //Getting a refference to the animator componnent we attached to the player
    private bool isFacingRight=true; //Here we declare a boolean that stores weather the character is facing right or not, true if starting position faces right
    private bool isWalking; //we need to keep track of weather the character is walking or not
    private bool isGrounded; //true if the player touches ground
    private bool canJump; // variable to check if we can jump(if true) or not(if false)
    private bool isTouchingWall;//boolean to store if the player is touching the wall or not
    private bool isWallSliding;//we want to check if character is touching wall and is not on the ground
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); //store a reference to the rigid body on the player 
        anim = GetComponent<Animator>();//Getting a refference to the animator componnent we attached to the player
        amountOfJumpsLeft = amountOfJumps;//Setting jumps left to the starting amount of times we can jump
        wallHopDirection.Normalize();//its going to make it so that the vector itself equals 1 so that when we multiply the components in to the force that we are going to add we can specify that force and it will always be that force.
        wallJumpDirection.Normalize();//
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();//Checks for input every frame
        CheckMovementDirection();
        UpdateAnimations();//calls updateAnimations function
        CheckIfCanJump();//calls CheckifCanJump function
        CheckIfWallSliding();
    }
    private void FixedUpdate()
    {
        ApplyMovement();//Calls Apply movement function to apply movement
        CheckSurroundings();//Calls checkSurroundings
    }
    private void CheckSurroundings()//Function that checks for the ground(or the wall)
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround); //Checks if a circle with the radius groundCheckRadius and the position of groundCheckPosition is touching the whatIsGround layer that we have set
        isTouchingWall = Physics2D.Raycast(wallCheck.position, -(transform.right), wallCheckDistance, whatIsGround);//We project the ray towards the right of the character so we use transform.right or -(transform.right)(depends on starting position?) ,then we need the distance  and the layer mask
    }
    private void CheckMovementDirection() //checks wheather or not the player is facing right and moving right and if its not the same it will flip the character
    {
        if(isFacingRight && movementInputDirection < 0) //if the character is facing right and the user input is -1 (trying to move left) then it will flip the character
        {
            Flip();//calls flip function to flip character
        }
        else if (!isFacingRight && movementInputDirection>0)//if the character is facing left and user input is 1 (trying to move right)then flip the character
        {
            Flip();
        }
        
        if(rb.velocity.x != 0)//Checks if the character is moving or not by checking his velocity on the x Axis.
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }
    private void CheckIfCanJump()//Checks if the player is able to jumb
    {
        if ((isGrounded && rb.velocity.y <= 0)||isWallSliding)//if player is grounded and is not jumping or is wallsliding(not moving up in the y axis)
        {
            amountOfJumpsLeft = amountOfJumps; //Resets the amount of jumps remaining to our starting amount
        }
        if (amountOfJumpsLeft == 0)//if out of jumps then can not jump
        {
            canJump = false;
        }
        else//can jump
        {
            canJump = true;
        }
    }
    private void UpdateAnimations()//function to update animations and animation variables and conditions
    {
        anim.SetBool("isWalking", isWalking);//with anim we access the parameters that we set up in the animator, we use SetBool because we have a boolean parameter, 'isWalking' is the name of the parameter we want to change, and we set it to the isWalking value
        anim.SetBool("isGrounded", isGrounded);//Updates the isGrounded condition in the animations
        anim.SetFloat("yVelocity", rb.velocity.y);//Updates the yVelocity variable in the animations
        anim.SetBool("isWallSliding", isWallSliding);//Updates the value of the isWallSliding animation parammeter to the isWallSliding value(true or false)
    }
    private void CheckInput() // this function will get called to check for any kind of input we might expect from the player
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal"); //horizontal is referenced to the A and D keys by default Raw makes it always return 0 or 1, without raw it would return a float between 0 and 1 the longer you hold the higher the number
        if(Input.GetButtonDown("Jump"))//Check if input key is pressed, its Space key by default
        {
            Jump();//Calls the function jump
        }
        
        if(Input.GetButtonUp("Jump"))//here we detect when the player lets go of the jump button(GetButtonUp is used to identify when the player releases the button)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);//we keep x velocity the same and we multiply the y velocity by our multiplier.
        }
    }
    private void CheckIfWallSliding()
    {
        if(isTouchingWall && !isGrounded && rb.velocity.y<0)//If Character is touching the wall and is not grounded and is not jumping up against the wall(so only when falling)
        {
            isWallSliding=true;
        }
        else
        {
            isWallSliding = false;
        }
    }
    private void Jump()//when the jump function is called we are just going to set the upwards velocity
    {
        if (canJump)//prevents infinite jumping(CheckIfCanJump function in Update)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);//keeps the same velocity on the x axis and changes the velocity of the y axis to jumpForce.(p.s. play with gravity scale on rigid body, maybe set to 4)
            amountOfJumpsLeft--;//Lowers the jumps left by 1 every time the player jumps
        }
        else if (isWallSliding && movementInputDirection==0 && canJump)//Wall Hop
        {
            isWallSliding = false;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -facingDirection, wallHopForce * wallHopDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        }
        else if ((isWallSliding || isTouchingWall) && movementInputDirection != 0 && canJump)//Touching wall sohe doesnt have to wait if he is moving up against the wall
        {
            isWallSliding = false;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        }
        
    }
    private void ApplyMovement()
    {
        if(isGrounded)//will only change velocity of the character is currently grounded (cant move while jumping use without if if you dont want it(also fixing it with variable movementForceInAir))
        {
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);//We keep the y axis the same and we change the x axis to move our character(rigid body) left and right
        }
        else if(!isGrounded && !isWallSliding && movementInputDirection!=0)//Not grounded, not wallsliding and has movement input direction
        {
            Vector2 forceToAdd = new Vector2(movementForceInAir * movementInputDirection, 0);//in the x axis we multiply movementForceInAir with movementInputDirection to apply force in both directions and in the y axis its gonna be 0 because we are only moving in the x direction
            rb.AddForce(forceToAdd);//adds force to rb
            if (Mathf.Abs(rb.velocity.x) > movementSpeed)//we want to clamp the velocity in the x direction again or else the velocity will keep increasing by adding more forces
            {
                rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);//we set the velocity directly again
            }
        }
        else if(!isGrounded && !isWallSliding && movementInputDirection == 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);//Here we fix the problem that the player doesnt stop moving when in the air and we stop giving input.
        }
        //rb.velocity = new Vector2(movementSpeed * movementInputDirection,rb.velocity.y);//We keep the y axis the same and we change the x axis to move our character(rigid body) left and right
        if(isWallSliding)
        {
            if(rb.velocity.y < -wallSlideSpeed) //-wallSlideSpeed for convenience so we can set wallSlideSpeed to a positive number Because the character will be moving downwards so if the velocity is bigger than the speed because they are both negative numbers
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);//we set the velocity to the wallSlideSpeed on the y axis and we leave the x axis as is
            }
        }
    }
    private void Flip()
    {
        if (!isWallSliding) //will only flip if not wallsliding
        {
            facingDirection *= -1;//Changes the direction we are facing(to 1 or -1)
            isFacingRight = !isFacingRight;// if its true it will become false and the oposite
            transform.Rotate(0.0f, 180.0f, 0);//flips the character by keeping x and z axis the same and rotating y axis by 180 degrees
        }
        //isFacingRight = !isFacingRight;// if its true it will become false and the oposite
        //transform.Rotate(0.0f, 180.0f, 0);//flips the character by keeping x and z axis the same and rotating y axis by 180 degrees
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius); //draws a shere
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));//We draw a gizmo so that we can see where we are checking for the wall
    }

}
