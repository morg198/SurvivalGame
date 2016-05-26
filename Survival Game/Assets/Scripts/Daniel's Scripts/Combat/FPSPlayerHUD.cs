namespace Player{
	using UnityEngine;
	using UnityEngine.UI;
	using System.Collections;

	public class FPSPlayerHUD : MonoBehaviour {

		[Header("Names / Assignment of GUI Elements")]
		public Text nameOfWeapon;
		public Text ammunitionCount;
        public Text fireMode;

        private string weaponName;
        private int currentAmmo;
        private int currentAmmoInMagazine;
        private string gunFireMode;

		// Use this for initialization
		void Start () {
			// Get our references for our objects
            nameOfWeapon = GameObject.Find("Gun Name").GetComponent<Text>();
			ammunitionCount = GameObject.Find("Gun Ammo Info").GetComponent<Text>();
            fireMode = GameObject.Find("Fire Mode").GetComponent<Text>();
		}

		// Update is called once per frame
		void Update () {
			// Show the currently equipped weapon name
            nameOfWeapon.text = weaponName;

            // Now show the current ammo count and the ammoMax * currentMagazine
            ammunitionCount.text = currentAmmoInMagazine + " | " + currentAmmo;

            // Set the fire mode of the current gun
            fireMode.text = gunFireMode;
		}

        public void setWeaponName(string name)
        {
            weaponName = name;
        }

        public void setCurrentAmmo(int ammo)
        {
            currentAmmo = ammo;
        }

        public void setCurrentAmmoInMagazine(int ammo)
        {
            currentAmmoInMagazine = ammo;
        }

        public void setGunFireMode(string mode)
        {
            gunFireMode = mode;
        }
	}
}