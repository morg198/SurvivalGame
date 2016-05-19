using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class FPSCharacterMotor : MonoBehaviour {

    [Header("Player")]
    public CharacterController player;

    [Header("Player Basic Movement")]
    public float playerSpeed = 20.0f;
    public float jumpingGravity = 7.0f;
    private float characterGravity = 0.0f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 5.0f;
    public float upDownRange = 60.0f;
    private float verticalRotation = 0.0f;

    [Header("Advanced Movement")]
    public float playerWalkingSpeed = 20.0f;
    public float playerRunSpeed = 40.0f;
    public float playerBackPedalSpeed = 7.0f;

    [Header("Stamina")]
    public bool playerStaminaSystem;
    public float playerStamina = 10.0f;
    public float maxStamina = 20.0f;
    private bool isRunning;
    private bool staminaCoolDownOver;
    private Rect staminaRect;
    private Texture staminaTexture;

    // Use this for initialization
    void Start () {
        // Getting the player
        player = this.GetComponent<CharacterController>();

        // lock the cursor the center of the game screen
        Cursor.lockState = CursorLockMode.Locked;

        // Stamina coold down is reset
        staminaCoolDownOver = true;
    }
	
	// Update is called once per frame
	void Update () {

        // Player CAMERA LOOK
        transform.Rotate(0, Input.GetAxis("Mouse X") * mouseSensitivity, 0);
        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

        // Player wants to RUN
        if (player.isGrounded && Input.GetKeyDown(KeyCode.LeftShift))
        {
            // check to see if the player stamina system is active
            if (playerStaminaSystem && staminaCoolDownOver)
            {
                playerSpeed = playerRunSpeed;

                isRunning = true;
            }
        }
        else
        {
            playerSpeed = playerWalkingSpeed;
        }

        // Player wants to JUMP
        if (player.isGrounded && Input.GetKey(KeyCode.Space))
        {
            characterGravity = jumpingGravity;
        }
        else
        {
            characterGravity += Physics.gravity.y * Time.deltaTime;
        }

        // Player is BACKPEDALLing
        if(player.isGrounded && Input.GetAxisRaw("Vertical") < 0)
        {
            playerSpeed = playerBackPedalSpeed;
        }
        else
        {
            playerSpeed = playerWalkingSpeed;
        }

        // Players STAMINA
        if (isRunning)
        {
            playerStamina -= Time.deltaTime;

            if(playerStamina < 0)
            {
                playerStamina = 0;
                isRunning = false;
                staminaCoolDownOver = false;
            }
        }
        else if(playerStamina < maxStamina)
        {
            if (playerStamina > 3)
            {
                staminaCoolDownOver = true;
            }

            playerStamina += Time.deltaTime;
        }

        // Player BASIC MOVEMENT
        Vector3 playerInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 direction = playerInput.normalized;

        direction = player.transform.TransformDirection(direction);

        Vector3 velocity = direction * playerSpeed;

        velocity.y = velocity.y + characterGravity;

        Vector3 moveAmount = velocity * Time.deltaTime;

        player.Move(moveAmount);
	}
}
