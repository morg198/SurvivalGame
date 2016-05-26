using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class FPSSimpleCharacterController : MonoBehaviour {

    // The Player
	private CharacterController characterController;
    private GameObject mainCamera;
	
	[Header ("Basic Movement")]
	public float movementSpeed = 5.0f;
    public float walkingSpeed = 5.0f;

    [Header("Jumping")]
    public float jumpHeight = 7.0f;
    private float characterGravity = 0;
	
	[Header ("Mouse Look")]
	public float mouseSensitivity = 5.0f;
	public float upDownRange = 60.0f;
	private float verticalRotation = 0;

    [Header("Advanced Movement")]
    public float runSpeed = 10;
    public bool isCrouching = false;

    private float characterHeight;
    private float characterCrouchingHeight;
	
	// Use this for initialization
	void Start () 
    {
        // Get Character references
		characterController = GetComponent<CharacterController>();
        mainCamera = GameObject.FindWithTag("MainCamera");

        // Set starting data
        characterHeight = characterController.height;
        characterCrouchingHeight = characterHeight / 2;
	}
	
	// Update is called once per frame
	void Update () 
    {	
        // Player Camera Rotation
		transform.Rotate(0, Input.GetAxis("Mouse X") * mouseSensitivity, 0);
		verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
		verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
		Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
		
        // Player wants to RUN
        if (characterController.isGrounded && Input.GetKey(KeyCode.LeftShift))
        {
            movementSpeed = runSpeed;
        }
        else
        {
            movementSpeed = walkingSpeed;
        }

        // Player wants to CROUCH
        if (Input.GetKeyDown(KeyCode.C))
        {
            Crouch();            
        }

		// Player wants to JUMP
		if(characterController.isGrounded && Input.GetKey(KeyCode.Space) ) 
        {
			characterGravity = jumpHeight;
        }
        else
        {
            characterGravity += Physics.gravity.y * Time.deltaTime;
        }
		

        // Player's Basic Movement
		Vector3 moveCharacter = new Vector3(Input.GetAxis("Horizontal") * movementSpeed, 
                                            characterGravity, 
                                            Input.GetAxis("Vertical") * movementSpeed);
		
		moveCharacter = transform.rotation * moveCharacter;
		
		characterController.Move(moveCharacter * Time.deltaTime);
	}

    /// <summary>
    /// This method is for making the character crouch. Very simple and quick.
    /// </summary>
    void Crouch()
    {
        // Check the state of crouching
        if (!isCrouching) // character is not crouching
        {
            // Make the character crouch
            characterController.height = characterCrouchingHeight;

            // Move the camera down to new height when crouching
            mainCamera.transform.localPosition -= new Vector3(0, characterCrouchingHeight, 0);

            // The character is now crouched
            isCrouching = true;
        }
        else
        {
            // Make the character stand back up
            characterController.height = characterHeight;

            // Move the camera back up to the new height
            mainCamera.transform.localPosition += new Vector3(0, characterCrouchingHeight, 0);

            // The character is now standing up
            isCrouching = false;
        }
    }
}
