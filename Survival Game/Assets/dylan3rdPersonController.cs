using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class dylan3rdPersonController : MonoBehaviour {

    [Header("Movement")]
    public float walkSpeed = 10;
    public float runSpeed = 15;
    public float backPedalSpeed = 5;
    public float jumpHeight = 5;

    [Header("Mouse Options")]
    public float xSensitivity = 5;
    public float ySensitivity = 5;
    float yawRot;

    float verticalVel = 0;
    float forwardVel = 0;
    float sideVel = 0;
    bool run = false;

    Vector3 velocity;

    //Will be used later to prevent camera from going through a wall
    Vector3 camOffset;

    CharacterController cc;
	void Start () {
        cc = gameObject.GetComponent<CharacterController>();
        camOffset = Camera.main.transform.localPosition;
        Cursor.visible = false;
	}
	
	void Update () {

        //Rotate character with mouse
        yawRot = Input.GetAxis("Mouse X");
        transform.Rotate(0, yawRot * xSensitivity, 0);

        //Give player a velocity 
        forwardVel = Input.GetAxis("Vertical");
        sideVel = Input.GetAxis("Horizontal");

        velocity = new Vector3(sideVel, verticalVel, forwardVel);
        velocity = transform.rotation * velocity;

        //Apply gravity to the player
        verticalVel += Physics.gravity.y * Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
        {
            if(cc.isGrounded)
            verticalVel = jumpHeight;
        }


        //Checks if the player is running
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            run = true;
        }
        
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            run = false;
        }
      
        //Run Diagonally forward
        if (Input.GetAxis("Vertical") > 0 && Input.GetAxis("Horizontal") != 0 && run)
        {


            /* Math for adjusted speed so walking diagonally is not faster than walking in a single direction
		
			 * 
			 * C^2 = A^2 + B^2
			 * 
			 * A = B  ( A is the Horizontal speed multiplier and B is Vertical speed multiplier, since we us the same multiplier "walkSpeed" for both they are equal )
			 * 
			 * C      ( C stands for our walkSpeed variable and as such we should solve for A in terms of C )
			 * 
			 * C^2 = A^2 + A^2    (Use the Pythagorean theorem to set up the equation to solve for what our new "A" or speed multiplier should be equal to)
			 * 
			 * C^2 = 2 * A^2
			 * 
			 * (C^2)/2 = A^2
			 * 
			 * A = sqrt(C^2/2)  
			 * 
			 * 
			 * 
			 * Notes for Confusion: C is equal to our original walkSpeed variable. A is what we want our new speed multiplier to be equal too
			 * 
			 */

            float adjustedSpeed = Mathf.Sqrt((runSpeed * runSpeed) / 2);
            cc.Move(velocity * adjustedSpeed * Time.deltaTime);

        }
        //Walk forward on a diagonal
        else if (Input.GetAxis("Vertical") > 0 && Input.GetAxis("Horizontal") != 0)
        {
            float adjustedSpeed = Mathf.Sqrt((walkSpeed * walkSpeed) / 2);
            cc.Move(velocity * adjustedSpeed * Time.deltaTime);

        }
        //Backpedal on a diagonal
        else if (Input.GetAxis("Vertical") < 0 && Input.GetAxis("Horizontal") != 0)
        {
            float adjustedSpeed = Mathf.Sqrt((backPedalSpeed * backPedalSpeed) / 2);
            cc.Move(velocity * adjustedSpeed * Time.deltaTime);

        }
        //Backpedal straight back
        else if (Input.GetAxis("Vertical") < 0)
        {
            cc.Move(velocity * backPedalSpeed * Time.deltaTime);

        }
        //Run straight
        else if (run)
        {
            cc.Move(velocity * runSpeed * Time.deltaTime);
        }
        //Walk straight
        else
        {
            cc.Move(velocity * walkSpeed * Time.deltaTime);
        }



    }
}
