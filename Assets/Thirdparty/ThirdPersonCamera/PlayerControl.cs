using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
#region Editor Fields
    //Serialize field lets us edit in editor window without making variables public
    [SerializeField]
    [Tooltip("Attached charactercontroller")]
    CharacterController controller;

    [SerializeField]
    [Tooltip("Player movement speed")]
    float speed;

    [SerializeField]
    [Tooltip("The desired jump height")]
    float jumpHeight;

    [SerializeField]
    [Tooltip("The desired jump time")]
    float jumpTime;

    [SerializeField]
    [Tooltip("Test target for the player to lock on to")]
    Transform testTarget;
    
    [SerializeField]
    [Tooltip("The camera looking at the player")]
    CameraControl mainCamera;
#endregion

#region Private Variables
    Transform target;
    float gravity;
    float jumpForce;
    float currentGravity;
    Vector3 moveVector;
    bool canJump;
#endregion

    void Start()
    {
        //Little math to detrmine our gravity and jump force to achieve desired jump arc
        gravity = (2 * jumpHeight) / Mathf.Pow((jumpTime / 2.0f), 2.0f);
		jumpForce = Mathf.Sqrt(2 * gravity * jumpHeight);
    }

    void Update()
    {
        MovementInput();
        IntegrateGravity();
        Jump();
        PerformMove();
        Targeting();
    }


    /*
     * Summary: 
     *      Test function to swap between untargeted and targeted
     */
    void Targeting() {
        if(Input.GetButtonDown("TargetLock")) {
            if(target != null) {
                target = null;
            } else {
                target = testTarget;
            }
            mainCamera.SetTarget(target);
        }
    }

    /*
     * Summary: 
     *      Updates current falling velocity
     */
    void IntegrateGravity() {
        if(controller.isGrounded) {
            currentGravity = 0;
            //Reset ability to jump
            canJump = true;
        }
        // Integrate gravity acceleration
        currentGravity -= gravity * Time.deltaTime;
    }

    /*
     * Summary: 
     *      Handles the actual movement of the character controller
     */
    void PerformMove() {
        moveVector.y = currentGravity;
        controller.Move(moveVector * Time.deltaTime * speed);
    }

    /*
     * Summary: 
     *      Handles jumping inputs and setup
     */
    void Jump() {
        if(canJump && Input.GetButtonDown("Jump")) {
            currentGravity = jumpForce;
            //Don't jump again until grounded again
            canJump = false;
        }
    }

    /*
     * Summary: 
     *      Handles player input to update player position
     */
    void MovementInput() {

        // Get Movement inputs
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");

        // Project camera forward onto xz plane
        Vector3 projectedFoward = mainCamera.transform.forward;
        projectedFoward.y = 0;
        projectedFoward.Normalize();

        // if the player is not targeting anything right now
        // just do typical third person camera movement
        if (target == null) {

            moveVector = horizontal * mainCamera.transform.right;
            moveVector += vertical * projectedFoward;
            moveVector.Normalize();
            
            transform.LookAt(transform.position + projectedFoward);
        } else {
        // Otherwise circle the target when moving left or right

            Vector3 targetVector = target.transform.position - transform.position;
            targetVector.y = 0;
            targetVector.Normalize();
            Vector3 rightVector = Vector3.Cross(Vector3.up, targetVector);

            moveVector = horizontal * rightVector;
            moveVector += vertical * targetVector;
            moveVector.Normalize();
            
            transform.LookAt(transform.position + targetVector);
        }
    }
}
