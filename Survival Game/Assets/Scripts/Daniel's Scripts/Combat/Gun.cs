namespace Weapons
{
    /**
        This gun script will control quite a few things. It will obviously control
        the guns, damage, fire range, fire rate. But it will also control the special
        effects of the gun. Like the guns animations, sounds, particle effects for this gun
        not for the enemy it shoots. In the game this code will go into, i expect the enemy
        to have its own hit effects. This script will also control, the guns firing mode;
        the semi-automatic fire, burst fire, and fully automatic fire.

        [4_30_2016] Debating on if this gun script should also control the guns GUI display
        as in the ammo count, name of gun, etc. 
    */


    using UnityEngine;
    using System.Collections;
    using System.Linq;

    public class Gun : MonoBehaviour
    {
        [Header("Name of Gun")]
        public string gunName;

        [Header("Gun Stats")]
        public int gunDamage;
        public int rangeOfGun;
        public int bulletPenetrationPower;
        public float knockBackForce;

        [Header("Bullets can shoot through walls")]
        public bool piercingBullets;
        public bool bulletPenetrationDebugMode;
        public float bulletTrailLife = 15.0f;
        private float surfacesShotThroughThreshold = 4;
        private int objectsEaseOfPenetration;
        private Vector3 objectsThickness;
        private RaycastHit[] obstaclesSortedNearest;

        [Header("Ammunition")]
        public int currentAmmo;
        public int ammoMax;
        public int currentAmmoInMagazine;
        public int magazineMax;
        private bool isReloading;

        public float weaponReloadTime = 4.0f;
        private float automaticModeFireRate = 7.0f;
        private float semiAutomaticModeFireRate = 0.25f;

        [Header("Firing Modes (Semi / Automatic")]
        public bool gunCanShootSemi;
        public bool gunCanShootAutomatic;
        public bool gunCanShootBurstFire;
        public enum FiringType { SemiAutomatic, Burst, Automatic };
        public FiringType FireMode = FiringType.SemiAutomatic;

        private int fireModeIndex = 0;
        private float singleShotFireRate = 0.0f;
        private float waitTimeTillNextFire = 1.0f;

        [Header("Burst Mode")]
        public int burstShots = 3;
        public float timeBetweenBursts = 0.1f;
        public float burstShotCoolDownTimer = 1.0f;
        private bool insideBurstShot;
        private WaitForSeconds timeBetweenBurstShots;
        private WaitForSeconds timeTillWeCanBurstFire;

        [Header("Gun Muzzle")]
        public Transform muzzle;

        [Header("Gun Effects")]
        public ParticleSystem hitParticlesEffect;
        WaitForSeconds shootingEffectsTimeLength = new WaitForSeconds(0.07f);

        [Header("Gun Animations")]
        public Animation animationController;

        [Header("Audio Clips")]
        public GameObject gunShotAudio;
        public GameObject gunDrawAudio;
        public GameObject gunReloadAudio;

        [Header("Crosshairs")]
        public Texture2D crosshairs;
        private Rect crosshairsPosition;

        [Header("Aim Down Sight")]
        public Vector3 hipPosition;
        public Vector3 adsPosition;
        public GameObject gunCamera;
        public float fpsCamFieldOfViewNormal = 60.0f;
        public float fpsCamFieldOfViewZoomed = 20.0f;
        public float cameraZoomSmooth = 12.5f;
        public float smoothAim = 12.5f;
        private bool isAiming;
        private Camera gunClippingCam;

        #region Initialization
        void Start()
        {
            // Get the players' eyes / gun's eyes
            gunCamera = GameObject.Find("Gun Camera");
            gunClippingCam = gunCamera.GetComponentInParent<Camera>();

            // Get the position of this gun's crosshairs
            crosshairsPosition = new Rect((Screen.width - crosshairs.width) / 2, 
                                          (Screen.height - crosshairs.height) / 2,
                                          crosshairs.width, 
                                          crosshairs.height);

            // Check if there is animations for this gun
            if(animationController != null)
            {
                // if there is, then grab it so we can play animations
                animationController = this.GetComponentInChildren<Animation>();
            }

            // Initialize some gun firing math
            timeBetweenBurstShots = new WaitForSeconds(timeBetweenBursts);
            timeTillWeCanBurstFire = new WaitForSeconds(burstShotCoolDownTimer);

            // Set the default of some features
            isAiming = false;
            isReloading = false;

            piercingBullets = false;
        }
        #endregion

        #region This GUNS LOGIC
        void Update()
        {
            // First we will see if the player is aiming this gun
            AimDownSight();

            // Find what type of firing mode we are in before shooting
            switch (FireMode)
            {
                // If its normal single fire, then shoot normal
                case FiringType.SemiAutomatic:

                    // Checking to see if the Player wants to shoot
                    if (Input.GetButtonDown("Fire1") && Time.time > singleShotFireRate && currentAmmoInMagazine != 0 && !isReloading)
                    {
                        singleShotFireRate = Time.time + semiAutomaticModeFireRate;

                        ShootGun();

                        // take away a bullet
                        currentAmmoInMagazine--;
                    }

                    if(currentAmmoInMagazine == 0 && !isReloading)
                    {
                        StartCoroutine(Reload());
                    }

                    break;

                case FiringType.Burst:

                    // Check to see if player is wanting to burst, and can burst
                    if (Input.GetButtonDown("Fire1") && Time.time > singleShotFireRate && !insideBurstShot && !isReloading)
                    {
                        singleShotFireRate = Time.time + semiAutomaticModeFireRate;

                        insideBurstShot = true;

                        // within this coroutine, the burst fire will take away the bullets shot
                        StartCoroutine(BurstFire());
                    }

                    // check to see if player has to reload
                    if (currentAmmoInMagazine == 0 && !isReloading)
                    {
                        StartCoroutine(Reload());
                    }

                    break;

                case FiringType.Automatic:

                    // Checking to see if the Player wants to unload on enemies
                    // He will be holding down the fire button, unlike the single fire
                    if (Input.GetButton("Fire1") && currentAmmoInMagazine != 0 && !isReloading)
                    {
                        if(waitTimeTillNextFire <= 0)
                        {
                            ShootGun();

                            // take away a bullet
                            currentAmmoInMagazine--;

                            waitTimeTillNextFire = 1;
                        }
                    }

                    if (currentAmmoInMagazine == 0 && !isReloading)
                    {
                        StartCoroutine(Reload());
                    }

                    waitTimeTillNextFire -= Time.deltaTime * automaticModeFireRate;

                    break;
            }

            // This is for allowing the player to change the gun's firing mode during the game
            if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                ChangeFireMode();
            }

            // Here we will check to see if the player is wanting to aim this gun
            if (Input.GetMouseButtonDown(1))
            {
                isAiming = !isAiming;
            }

            // And lastly for the gun class we will see if the player wants to reload his gun manually
            if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmoInMagazine != magazineMax)
            {
                StartCoroutine(Reload());
            }
        }
        #endregion

        #region Basic Shot
        private void ShootGun()
        {
            // Gonna see if our players bullets hit anything
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));


            // This is more for demo purposes, to see if bullets can pierce through walls
            if (piercingBullets)
            {
                ShootWithPiercingBullets(ray);
            }
            //----------------------------------------------------------------------------


            // Now Checking to see if the Player has hit anything
            if (Physics.Raycast(ray, out hit, rangeOfGun))
            {
                // if the enemy has been hit we want to knock them back a bit
                if (hit.collider.tag == "Enemy")
                {
                    if (hit.rigidbody != null)
                    {
                        hit.rigidbody.AddForce(-hit.normal * knockBackForce);
                    }

                    // Make the enemy take some damage for that hit too
                    AI.AILife enemieLife = hit.collider.GetComponent<AI.AILife>();
                    if (enemieLife != null)
                    {
                        enemieLife.TakeDamage(gunDamage, hit.point, hit.transform.forward);
                    }
                }
                // if the enemy hasnt been hit but just shooting the gun to hit other stuff, fire off particle effects
                else
                {
                    if (hit.collider.tag != "Player")
                    {
                        Destroy(Instantiate(hitParticlesEffect, hit.point, Quaternion.identity) as GameObject, hitParticlesEffect.startLifetime);
                    }
                }

            }

            // now start the Guns Special Effects
            StartCoroutine(ShotEffects());
        }

        IEnumerator ShotEffects()
        {
            // if we have audio we could play that here along with the animations
            PlayAudio("fire");

            // play the animation aswell
            PlayAnimation("fire");

            // wait time before we can fire the gun again
            yield return shootingEffectsTimeLength;
        }
        #endregion

        #region Shooting Through Walls/Obstacles
        private void ShootWithPiercingBullets(Ray bulletsPath)
        {
            // DEBUG MODE - Going to allow us to see visually what we hit
            if (bulletPenetrationDebugMode)
            {
                ShowPiercingBulletTrail(bulletsPath);
            }

            // This will allow us mathematically to see what we hit
            RaycastHit[] obstaclesInTheBulletsWay;
            obstaclesInTheBulletsWay = Physics.RaycastAll(bulletsPath, rangeOfGun);

            // So first we have to setup our wall/obstacle pierce count
            int surfaces = 0;

            // get our guns piercing power
            int penetrationPower = bulletPenetrationPower;

            // need to loop through the obstacles and see which is the first one hit
            obstaclesInTheBulletsWay = obstaclesInTheBulletsWay.OrderBy(h => h.distance).ToArray();

            // Time to loop through all of the objects hit and see where the bullet should stop
            for (int i=0; i < obstaclesInTheBulletsWay.Length; i++)
            {
                // DEBUG MODE - Checking to see if we are hitting walls in correct order
                if (bulletPenetrationDebugMode)
                {
                    print("Obstacle " + i + " : " + obstaclesInTheBulletsWay[i].transform.name);
                }

                // first check to see if the bullet can pierce through the wall / obstacle
                if(penetrationPower <= 0 && surfaces >= surfacesShotThroughThreshold)
                {
                    if (bulletPenetrationDebugMode)
                    {
                        print("Wall can not be pierced.");
                    }

                    // wall cannot be pierced
                    return;
                }

                // The bullet has pierced through an obstacle
                surfaces++;

                // Now to get the objects material and thickness
                RaycastHit objectHit = obstaclesInTheBulletsWay[i];
                GetObjectToPierceThrough(objectHit);

                // Calculate the reduced dps of the gun
                

                // bullet is piercing out of the obstacle, exiting obstacle
                surfaces++;
            }
        }

        private void ShowPiercingBulletTrail(Ray bulletTrail)
        {
            // Create a new temporary gameobject so we can see our line
            GameObject tempGO = new GameObject();

            // Create the line and attach it to that gameobject
            LineRenderer lineRenderer = tempGO.AddComponent<LineRenderer>();

            // Change the colors of the line so we can see it better
            lineRenderer.material = new Material((Shader.Find("Particles/Additive")));
            lineRenderer.SetColors(Color.red, Color.red);

            // Set the size of the bullet trail
            lineRenderer.SetWidth(0.1f, 0.1f);

            // Now show the bullet trail in the game
            lineRenderer.SetPosition(0, bulletTrail.origin);
            lineRenderer.SetPosition(1, bulletTrail.GetPoint(rangeOfGun));

            // Destory this object after a certain amount of time
            Destroy(tempGO, bulletTrailLife);
        }

        private void GetObjectToPierceThrough(RaycastHit obstacle)
        {
            // Gonna get the object's ease of penetration
            if(obstacle.transform.tag == "Glass" || obstacle.transform.tag == "Cardboard")
            {
                objectsEaseOfPenetration = 1;
            }

            if (obstacle.transform.tag == "Metal Grate" || obstacle.transform.tag == "Wood")
            {
                objectsEaseOfPenetration = 2;
            }

            if (obstacle.transform.tag == "Plaster" || obstacle.transform.tag == "Tile")
            {
                objectsEaseOfPenetration = 3;
            }

            if (obstacle.transform.tag == "Metal" || obstacle.transform.tag == "Concrete" || obstacle.transform.tag == "Brick")
            {
                objectsEaseOfPenetration = 4;
            }

            if (obstacle.transform.tag == "Solid Metal")
            {
                objectsEaseOfPenetration = 5;
            }

            // next get the size of the object
            objectsThickness = obstacle.collider.GetComponent<Renderer>().bounds.size;

        }
        #endregion

        #region Firing Modes
        private void ChangeFireMode()
        {
            // Check the firing mode
            if (fireModeIndex == 0)
            {
                FireMode = FiringType.SemiAutomatic;

                Debug.Log("Fire Mode Set to " + FireMode);
            }
            else if (fireModeIndex == 1)
            {
                FireMode = FiringType.Burst;

                Debug.Log("Fire Mode Set to : " + FireMode);
            }
            else if (fireModeIndex == 2)
            {
                FireMode = FiringType.Automatic;

                Debug.Log("Fire Mode Set to : " + FireMode);

                fireModeIndex = -1;
            }

            fireModeIndex++;
        }

        IEnumerator BurstFire()
        {
            for(int shots = 0; shots < burstShots; shots++)
            {
                // Check to see if we can shoot gun or if we should quit and reload immediateley
                if(currentAmmoInMagazine == 0)
                {
                    insideBurstShot = false;

                    yield break;
                }

                ShootGun();

                // take away a bullet
                currentAmmoInMagazine--;

                yield return timeBetweenBurstShots;
            }

            yield return timeTillWeCanBurstFire;
            
            // After we are done with our burst shot, we can allow the player to shoot again
            insideBurstShot = false;
        }

        #endregion

        #region Aiming
        private void AimDownSight()
        {
            // We are aiming
            if (isAiming)
            {
                // gonna slowly move the gun into the aiming position
                this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, adsPosition, Time.deltaTime * smoothAim);

                // then slowly zoom the camera in, so it feels like we are truly aiming
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, fpsCamFieldOfViewZoomed, Time.deltaTime * cameraZoomSmooth);

                // then disable the gun clipping/culling mask camera
                gunClippingCam.enabled = false;
            }

            // We are not aiming
            if (!isAiming)
            {
                // since we are not aiming, gonna move the gun back to its normal position
                this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, hipPosition, Time.deltaTime * smoothAim);

                // then slowly zoom the camera back out, since we are no longer aiming
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, fpsCamFieldOfViewNormal, Time.deltaTime * cameraZoomSmooth);

                // turn back on the gun clipping mask
                if(Camera.main.fieldOfView == fpsCamFieldOfViewNormal)
                {
                    gunClippingCam.enabled = true;
                }
            }
        }
        #endregion

        #region Reloading Gun
        IEnumerator Reload()
        {
            // Disable the aiming
            isAiming = false;

            // Want to check 
            // if the current ammo in the magazine is greater than or equal to 0
            // if the current ammo in the magazine is less than the maximum ammo 
            // if the current ammo is greater than 0
            if (currentAmmoInMagazine >= 0 && currentAmmoInMagazine < magazineMax && currentAmmo > 0)
            {
                isReloading = true;

                // Play reload sound
                PlayAudio("reload");

                // Play reload animation
                PlayAnimation("Reload");

                // Now wait the reload time for the weapon
                yield return new WaitForSeconds(weaponReloadTime);

                // Now actually reload the gun
                for (int i = 0; i < magazineMax; i++)
                {
                    if (currentAmmoInMagazine == magazineMax || currentAmmo <= 0)
                    {
                        break;
                    }
                    else
                    {
                        currentAmmoInMagazine++;
                        currentAmmo--;
                    }
                }

                isReloading = false;
            }
            else
                // Check the case that we have ammo in the magazine but its the players last clip
                if (currentAmmoInMagazine > 0 && currentAmmo == 0)
                    isReloading = false;

            yield break;
        }
        #endregion

        #region Audio and Animation
        public void PlayAudio(string audioClip)
        {
            // See what audio we want to play			
            switch (audioClip)
            {
                // Check if we want the Weapon Draw audio
                case "weapon equip":

                    // Make sure we have that audio connected for the gun
                    if (gunDrawAudio != null)
                    {
                        GameObject gunDeploy = Instantiate(gunDrawAudio, this.transform.position, this.transform.rotation) as GameObject;
                    }

                    break;

                // Check if we want the Weapon Reload audio
                case "reload":

                    // Make sure we have that audio connected for the gun
                    if (gunReloadAudio != null)
                    {
                        GameObject gunReload = Instantiate(gunReloadAudio, this.transform.position, this.transform.rotation) as GameObject;
                    }

                    break;

                // Check if we want the Weapon Fire audio
                case "fire":

                    // Make sure we have that audio connected for the gun
                    if (gunShotAudio != null)
                    {
                        GameObject gunFire = Instantiate(gunShotAudio, this.transform.position, this.transform.rotation) as GameObject;
                    }

                    break;
            }
        }

        public void PlayAnimation(string anim)
        {
            // See what animation we want to play			
            switch (anim)
            {
                // Check if we want the Weapon Draw Animation
                case "draw":
                    anim = "draw";
                    break;

                // Check if we want the Weapon Reload Animation
                case "reload":
                    anim = "reload";
                    break;

                // Check if we want the Weapon Fire Animation
                case "fire":
                    anim = "fire";
                    break;

                // Check if we want the Weapon Idle Animation
                case "idle":
                    anim = "idle";
                    break;
            }

            // Play the animation.
            if (animationController != null)
            {
                if (animationController.IsPlaying(anim) == false)
                    animationController.Play(anim);
                else if (animationController.IsPlaying(anim) == true)
                    animationController.Rewind(anim);
            }
        }
        #endregion

        #region Guns GUI
        void OnGUI()
        {
            // For our gun's GUI, if the player is not aiming, draw the crosshairs
            if(!isAiming)
                GUI.DrawTexture(crosshairsPosition, crosshairs);
        }
        #endregion

        #region Getters and Setters
        public string getWeaponName()
        {
            return gunName;
        }

        public int getCurrentAmmo()
        {
            return currentAmmo;
        }

        public int getAmmoMax()
        {
            return ammoMax;
        }

        public int getCurrentAmmoInMagazine()
        {
            return currentAmmoInMagazine;
        }

        public int getMagazineMax()
        {
            return magazineMax;
        }

        public string getFiringMode()
        {
            return FireMode.ToString();
        }
        #endregion
    }
} 