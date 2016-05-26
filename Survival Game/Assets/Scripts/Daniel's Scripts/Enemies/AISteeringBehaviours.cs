namespace AI
{
    using UnityEngine;
    using System.Collections;

    public class AISteeringBehaviours : MonoBehaviour
    {
        /**
            This class is for AI Steering Behaviours. It is for adding to an AI 
            component and allowing that component to have such behaviours as seek/pursue, 
            evade, hide, wander, etc.
                
            This particular version of the steering behaviours is for simple zombie behaviours
                idle, seek, wander, obstacle avoidance
        */

        [Header("The target of the AI (Player)")]
        public Transform target;                    // the AI's target (player)
        public float attackDistance;
        public float targetEscapeDistance = 25.0f;
        public float targetsSlowingRadius = 5.0f;   // the range of the ai to player before it starts to slow down and arrive

        [Header("The AI's Movement")]
        public float AISpeed = 1.0f;                // the speed the AI moves to chase the player
        public float AIRotationSpeed = 4.0f;        // the speed the ai rotates to look at player
        private Vector3 steering;                   // the vector the pushes the ai in the appropriate behaviours direction
        private Vector3 currentVelocity;            // the current velocity of the AI
        
        public enum AIState { Idle, Seek, Wander};
        public AIState AIStateBehaviour;

        [Header("Idle Attributes")]
        public float idleToWanderTimer = 5.0f;
        public float seekDistance = 10.0f;
        public float idleTimer = 5.0f;

        [Header("Seek / Arrive Attributes")]
        public float deceleratingAISpeed;           // the speed the AI slows to while getting close to the player
        public float seekToWanderTimer = 5.0f;
        public float seekTimer = 5.0f;

        [Header("Wander Attributes")]
        public float AIWanderSpeed = 2.0f;          // speed when AI is wandering in case the user wants it to be different
        public float circlesDistanceFromAI = 3f;    // the circles distance from the AI player
        public float circlesRadius = 3f;
        public float randomAngle = 0.0f;
        public float timeTillWanderAngleChange = 3.0f;
        private float wanderTimer = 3.0f;

        [Header("Collision Avoidance Attributes")]
        public float distanceAICanSeeAhead = 10.0f;
        public float obstacleAvoidancePushAway = 0.01f;

        [Header("Animations For Current AI")]
        public AIAnimationControl animationControl;

        #region Initilization
        /// <summary>
        /// Used for the initialization of all the AI characters this is currently attached to.
        /// </summary>
        void Start()
        {
            // get the AI's target (player)
            target = GameObject.FindWithTag("Player").transform;

            // set the attack distance so the ai knows when to stop
            attackDistance = 1.3f;

            // the current AI's velocity
            currentVelocity = Vector3.zero;

            // the steering of the AI
            steering = Vector3.zero;

            // set the ai to idle to start out
            AIStateBehaviour = AIState.Idle;

            // animations
            if(animationControl != null)
            {
                animationControl = GetComponent<AIAnimationControl>();
            }
        }
        #endregion

        #region Movement
        /// <summary>
        /// This is where if we want the behaviours to be updated it will be run through this method.
        /// </summary>
        void Update()
        {
            // We will update through the behaviours so that at the end of the update function
            // the AI will be moving according to its behavior.
            switch (AIStateBehaviour)
            {
                case AIState.Idle:
                    Idle();

                    // if there are animations
                    if(animationControl != null)
                    {
                        animationControl.playLocomotionAnimation("idle");
                    }

                    break;
                case AIState.Seek:
                    Seek();

                    // if there are animations
                    if (animationControl != null)
                    {
                        animationControl.playLocomotionAnimation("run");
                    }

                    break;
                case AIState.Wander:
                    Wander();

                    // if there are animations
                    if (animationControl != null)
                    {
                        animationControl.playLocomotionAnimation("walk");
                    }

                    break;
            }

            // check for obstacles in the way, then update agent (AI)
            CollisionAvoidance();

            // move the AI
            this.transform.position += steering;
        }
        #endregion

        #region Steering Behaviours
        /// <summary>
        /// Idle Function - pretty much AI isnt moving so keep the velocity set to zero.
        /// </summary>
        private void Idle()
        {
            // Ai is idle so velocity should equal zero, not moving
            steering = Vector3.zero;

            // Check to see if we should be seeking the player if player is close
            float distanceToPlayer = Vector3.Distance(target.transform.position, this.transform.position);

            if (distanceToPlayer <= seekDistance)
            {
                setAIBehaviourState("Seek");
                idleTimer = idleToWanderTimer;
            }

            // Check to see if the idle timer has elapsed, agent is out of combat and is dormant, then wander
            if(idleTimer <= 0){
                setAIBehaviourState("Wander");
                idleTimer = idleToWanderTimer;
            }

            // Count down the timer so that the agent isnt just staying idle the whole time
            idleTimer -= Time.deltaTime;
        }

        /// <summary>
        /// This function will seek the target with arrival code. This will return the Vector3 velocity that the 
        /// AI should take to seek the target then slowly arrive at player.
        /// </summary>
        private void Seek()
        {
            // get a fresh vector to return, which will be the desired velocity (steering velocity)
            Vector3 desired_velocity = Vector3.zero;

            // get the direction of the player from the AI
            Vector3 direction = target.position - this.transform.position;

            // keep the ai grounded
            direction.y = 0;

            // now look at the player before giving chase
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), AIRotationSpeed * Time.deltaTime);

            // get the distance from the target to the player
            float distance = Vector3.Distance(target.transform.position, this.transform.position);

            // check to see if the ai is close to the player so we can arrive, or continue to seek
            if (distance > targetsSlowingRadius && distance <= targetEscapeDistance)
            {
                // get the normal seek velocity for the AI
                desired_velocity = direction.normalized * AISpeed * Time.deltaTime;
            }
            else if (distance <= targetsSlowingRadius && distance > attackDistance)
            {
                // since the ai is close to the player get our decelerating speed
                deceleratingAISpeed = distance / 5;

                float speed = AISpeed * deceleratingAISpeed;

                // get the new desired velocity of the AI with the behaviour modification
                desired_velocity = direction.normalized * speed * Time.deltaTime;

                //print(distance);
            }
            else if (distance <= attackDistance)
            {
                // stoping the AI
                desired_velocity = Vector3.zero;

                // the agent has been idle long enough, just make it wander
                if (seekTimer <= 0)
                {
                    setAIBehaviourState("Wander");
                    seekTimer = seekToWanderTimer;
                }

                // if the agent (AI) has been idle long enough, make it go wander around
                seekTimer -= Time.deltaTime;
            }

            // steering force is the result of the desired velocity subtracted by the current velocity 
            // and it pushes the character towards the target as well
            steering = desired_velocity - currentVelocity;
        }

        /// <summary>
        /// This method will control the AI's wander feature, if the AI is not chasing the player
        /// then the AI will just wander around.
        /// </summary>
        private void Wander()
        {
            // Clean vector to come back to
            Vector3 wanderForce = Vector3.zero;

            // Get the CIRCLES CENTER
            // the circle should have the same velocity as the agent (AI)
            Vector3 circlesCenter = new Vector3(this.transform.forward.x,
                                                this.transform.forward.y,
                                                this.transform.forward.z);
            circlesCenter.Normalize();
            circlesCenter *= circlesDistanceFromAI;


            // Now Get the DISPLACEMENT FORCE (determine's if our agent (AI) goes left or right)
            Vector3 displacement = new Vector3(0, 0, 1); ;
            displacement *= circlesRadius;

            //Debug.DrawRay(circlesCenter, displacement, Color.red);

            // Get the RANDOM ANGLE, which will determine our wandering position
            if (wanderTimer <= 0)
            {
                randomAngle = (float)Random.Range(0, 360);
                wanderTimer = timeTillWanderAngleChange;
            }
            wanderTimer -= Time.deltaTime;
            
            setAngle(ref displacement, randomAngle);


            // Now Get the WANDER FORCE; Now add it all up (displacement force + current velocity) to equal our wander force
            wanderForce = (this.transform.forward * AIWanderSpeed) + (displacement + circlesCenter);
            wanderForce.Normalize();
            wanderForce *= AIWanderSpeed * Time.deltaTime;

            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(wanderForce), AIRotationSpeed * Time.deltaTime);
            wanderForce.y = 0;

            steering = wanderForce;

            // Check to see if the AI is close to the target/player, if so we can stop wandering and hunt the target/player
            float targetsDistance = Vector3.Distance(target.transform.position, this.transform.position);
            if(targetsDistance <= seekDistance){
                setAIBehaviourState("Seek");
            }
        }

        private void CollisionAvoidance(){
            // will need three raycasts to check the agents surrounding area
            RaycastHit forward;
            RaycastHit left;
            RaycastHit right;

            Vector3 directionToTheRight = transform.TransformDirection(new Vector3(Mathf.Cos(45 * Mathf.Deg2Rad), 0, Mathf.Sin(45 * Mathf.Deg2Rad)));
            Vector3 directionToTheLeft = transform.TransformDirection(new Vector3(-Mathf.Cos(45 * Mathf.Deg2Rad), 0, Mathf.Sin(45 * Mathf.Deg2Rad)));

            // FORWARD - gonna see if there is anything directly infront of the agent
            if (Physics.Raycast(this.transform.position, this.transform.forward, out forward, distanceAICanSeeAhead))
            {
                //make sure the ray isnt hitting the agent it is inside
                if(forward.transform != this.transform && forward.transform != target.transform){
                    // then update the player to avoid the obstacle
                    steering += forward.normal * obstacleAvoidancePushAway;
                }
            }
            //Debug.DrawRay(this.transform.position, this.transform.forward, Color.red);

            // RIGHT - gonna see if there is anything diagonally infront of the agent to the right
            if (Physics.Raycast(this.transform.position, directionToTheRight, out right, distanceAICanSeeAhead))
            {
                //make sure the ray isnt hitting the agent it is inside
                if (forward.transform != this.transform && forward.transform != target.transform)
                {
                    // then update the player to avoid the obstacle
                    steering += forward.normal * obstacleAvoidancePushAway;
                }
            }
            //Debug.DrawRay(this.transform.position, directionToTheRight, Color.magenta);

            // LEFT - gonna see if there is anything diagonally infront of the agent to the right
            if (Physics.Raycast(this.transform.position, directionToTheLeft, out left, distanceAICanSeeAhead))
            {
                //make sure the ray isnt hitting the agent it is inside
                if (forward.transform != this.transform && forward.transform != target.transform)
                {
                    // then update the player to avoid the obstacle
                    steering += forward.normal * obstacleAvoidancePushAway;
                }
            }
            //Debug.DrawRay(this.transform.position, directionToTheLeft, Color.green);
        }
        #endregion

        #region Public Class (Set Behaviour State)
        /// <summary>
        /// Other classes can access this method so that they can change the behaviour of the AI
        /// </summary>
        /// <param name="state"></param>
        public void setAIBehaviourState(string state)
        {
            switch (state)
            {
                case "Idle":
                    AIStateBehaviour = AIState.Idle;
                    break;
                case "Seek":
                    AIStateBehaviour = AIState.Seek;
                    break;
                case "Wander":
                    AIStateBehaviour = AIState.Wander;
                    break;
            }
        }
        #endregion

        #region Helper Classes
        private Vector3 getRandomPointOnACircle(Vector3 center, float radius, float angle)
        {
            Vector2 cartesianCoordinates;

            cartesianCoordinates.x = center.x + (radius * Mathf.Cos(angle * Mathf.Deg2Rad));
            cartesianCoordinates.y = center.z + (radius * Mathf.Sin(angle * Mathf.Deg2Rad));

            // DEBUGGING----------------------------------------------
            //print("Cartesian Coordinates : " + cartesianCoordinates);
            //--------------------------------------------------------
            return new Vector3(cartesianCoordinates.x, 0 , cartesianCoordinates.y);
        }

        private void setAngle(ref Vector3 vector, float angle){
            vector.x = Mathf.Cos(angle * Mathf.Deg2Rad) * circlesRadius;
            vector.z = Mathf.Sin(angle * Mathf.Deg2Rad) * circlesRadius;
        }
        #endregion

        #region Getters and Setters
        public AIState getAIState()
        {
            return AIStateBehaviour;
        }
        #endregion
    }
}