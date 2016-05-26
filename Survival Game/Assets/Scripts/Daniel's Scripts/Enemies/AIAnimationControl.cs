using UnityEngine;
using System.Collections;

public class AIAnimationControl : MonoBehaviour {

    // Animations
    Animator anim;

    // Quick references for optimization
    int dyingState;
    int idleState;
    int walkingState;
    int runningState;

    // Use this for initialization
    void Start () {
        // Grab the Animator controller to control animations
        anim = GetComponentInChildren<Animator>();

        dyingState = Animator.StringToHash("Base Layer.zombie_death");
        idleState = Animator.StringToHash("Base Layer.zombie_idle");
        walkingState = Animator.StringToHash("Base Layer.zombie_walk");
        runningState = Animator.StringToHash("Base Layer.zombie_run");
	}

    public void playLocomotionAnimation(string animationName)
    {
        int state;

        // See what animation we want to play			
        switch (animationName)
        {
            // Check if we want the Weapon Idle Animation
            case "idle":
                state = idleState;
                break;

            case "walk":
                state = walkingState;
                break;

            case "run":
                state = runningState;
                break;

            default:
                state = idleState;
                break;
        }

        // Play the animation.
        if (anim != null)
        {
            anim.Play(state);
        }
    }

    public void playDeathAnimation(bool isDead)
    {
        if (isDead)
        {
            anim.Play(dyingState);
        }
    }

    #region Getters and Setters
    public float getDeathAnimationPlayTime()
    {
        float playTime = anim.playbackTime;

        return playTime;
    }
    #endregion
}