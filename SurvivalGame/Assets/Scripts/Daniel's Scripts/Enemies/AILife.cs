namespace AI
{
    using UnityEngine;
    using System.Collections;

    public class AILife : MonoBehaviour
    {
        [Header("AI's Health")]
        public float health = 100.0f;
        private float currentHealth;
        private float deathTimer;

        [Header("AI's Animation")]
        public Animator animatorController;

        [Header("AI's Special Effects")]
        public ParticleSystem hitMarkerParticleEffect;
        public ParticleSystem deathParticleEffect;

        [Header("AI's Home - Spawner")]
        public GameObject spawnManager;
        //private EnemySpawnManager spawnScript;

        /// <summary>
        /// This method is used for initialization in Unity
        /// </summary>
        void Start()
        {
            // Set this AI's health
            currentHealth = health;

            // Check to see if there is a spawn manager for this enemy
            if(spawnManager != null)
            {
                spawnManager = GameObject.Find("Spawner");
                //spawnScript = spawnManager.GetComponent<EnemySpawnManager>();

            }

            // Check to see if this AI should have animations or not
            if(animatorController != null)
            {
                animatorController = GetComponentInChildren<Animator>();
            }
        }

        /// <summary>
        /// This method is for the AI to take damage and pretty much for it to die
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection)
        {
            // The Ai has taken damage,
            Debug.Log("I have been hit "); 
            currentHealth -= damage;

            // Now if we have particles added to the current AI, show those particles
            if(hitMarkerParticleEffect != null)
            {
                // we are gonna show those particles and also delete the at the same time
                Destroy(Instantiate(hitMarkerParticleEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, hitMarkerParticleEffect.startLifetime);
            }

            // If the AI should be dead
            if (currentHealth <= 0)
            {
                if(deathParticleEffect != null)
                {
                    Destroy(Instantiate(deathParticleEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathParticleEffect.startLifetime);
                }

                if(animatorController != null)
                {
                    //AIAnimationControl animationController = GetComponent<AIAnimationControl>();

                    //animationController.playDeathAnimation(true);                 
                }

                Dead();

                // now if this enemy is part of a spwaner you can uncomment this code
                if (spawnManager != null)
                {
                    //spawnScript.killEnemy();
                }
            }
        }

        void Dead()
        {
            // Destroy the scripts of this AI (movement and colliders) and then destory this AI
            Destroy(GetComponent("AISteeringBehaviours"));
            Destroy(GetComponent<Rigidbody>());
            Destroy(GetComponent<CapsuleCollider>());
            Destroy(this);

            Destroy(gameObject, 2);
        }
    }
}