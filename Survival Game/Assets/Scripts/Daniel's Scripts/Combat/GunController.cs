namespace Weapons
{
    using UnityEngine;
    using System.Collections;

    public class GunController : MonoBehaviour
    {
        [Header("The Players Hands - Weapon Spawn Point")]
        public Transform weaponHolder;
        public Gun startingGun;
        Gun equippedGun;

        [Header("List of guns to use")]
        public Gun[] guns;
        private int currentGunIndex;

        Player.FPSPlayerHUD playerHUD;

        void Start()
        {
            // get a reference to the Players player hud
            //playerHUD = GameObject.Find("FPS Player Hud").GetComponent<Player.FPSPlayerHUD>();

            if (startingGun != null)
            {
                EquipGun(startingGun);
                currentGunIndex = 0;
            }
        }

        void Update()
        {
            // Gun controller will handle if the player or ai wants to swap guns
            if (Input.GetKeyDown(KeyCode.E))
            {
                // see if we can swap weapons
                if(currentGunIndex + 1 < guns.Length)
                {
                    // since we can now swap weapons, set the gun index to the new current weapon
                    currentGunIndex++;

                    // equip our new gun
                    EquipGun(guns[currentGunIndex]);
                }
            }

            // Checking to see if player wants to swap back to the previous gun/weapon
            if(Input.GetKeyDown(KeyCode.Q))
            {
                // see if we can swap weapons
                if (currentGunIndex - 1 >= 0)
                {
                    // since we can now swap weapons, set the gun index to the new current weapon
                    currentGunIndex--;

                    // equip our new gun
                    EquipGun(guns[currentGunIndex]);
                }
            }

            // set the update the gun data into the GUI
            //setGunsGUI();
        }

        public void EquipGun(Gun gunToEquip)
        {
            // Check to see if there is a gun in the players hands already
            if (equippedGun != null)
            {
                // if so, delete that gun
                Destroy(equippedGun.gameObject);
            }

            // Equip the gun into the Players hand
            equippedGun = Instantiate(gunToEquip, weaponHolder.position, weaponHolder.rotation) as Gun;

            // Then parent the equipped gun the player's hands so it moves with the player
            equippedGun.transform.parent = weaponHolder;

            // Play the audio to euip/draw weapon
            equippedGun.PlayAudio("weapon equip");

            // Playing animation if the gun is CZ 805 to change weapons
            equippedGun.PlayAnimation("Draw");
        }

        private void setGunsGUI()
        {
            // Set the guns weapon name
            playerHUD.setWeaponName(equippedGun.getWeaponName());

            // Set the current ammo in the clip
            playerHUD.setCurrentAmmoInMagazine(equippedGun.getCurrentAmmoInMagazine());

            // set the current amount of ammo
            playerHUD.setCurrentAmmo(equippedGun.getCurrentAmmo());

            // Set the firing mode for the gun
            playerHUD.setGunFireMode(equippedGun.getFiringMode());
        }
    }
}