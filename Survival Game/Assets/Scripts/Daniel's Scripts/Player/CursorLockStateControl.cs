using UnityEngine;
using System.Collections;

public class CursorLockStateControl : MonoBehaviour {

    private bool isCursorLocked;

	// Use this for initialization
	void Start () {
        ToggleCursorState();
	}
	
	// Update is called once per frame
	void Update () {
        CheckForCursorLockSwap();
        CheckIfCursorShouldBeLocked();
	}

    void ToggleCursorState()
    {
        isCursorLocked = !isCursorLocked;
    }

    void CheckForCursorLockSwap()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorState();
        }
    }

    void CheckIfCursorShouldBeLocked()
    {
        if (isCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
