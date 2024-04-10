using System.Collections;
using System.Collections.Generic;
using static PlayerGun;
using static weaponSwitching;
using UnityEngine;

public class ReticleEnlargement : MonoBehaviour
{
    [Header("Definitions")]
    public string[] gunObjNames;
    private PlayerGun gunObj;

    [Header("Reticle")]
    public float reticleSizeMultiplier;
    public float minimumSize;
    public Component moveComponent;
    public weaponSwitching weaponSwitchingObj;
    private int currentWeaponInt = 0;




    public enum Component {
        negativeHorizontal,
        positiveHorizontal,
        negativeVertical,
        positiveVertical
    }


    void Awake()
    {
        // assign the gun object
        currentWeaponInt = weaponSwitchingObj.selectedWeapon;

        gunObj = GameObject.Find(gunObjNames[currentWeaponInt])?.GetComponent<PlayerGun>();
        if (gunObj == null) UnityEngine.Debug.LogError($"Cannot find PlayerGun object of name {gunObjNames[currentWeaponInt]}");
    }

    void Update()
    {
        // get new gun obj
        currentWeaponInt = weaponSwitchingObj.selectedWeapon;
        gunObj = GameObject.Find(gunObjNames[currentWeaponInt])?.GetComponent<PlayerGun>();
        if (gunObj == null) UnityEngine.Debug.LogError($"Cannot find PlayerGun object of name {gunObjNames[currentWeaponInt]}");


        else {
            var ADS = gunObj.ADSEnabled;
            float reticleSize;

            // bepaal hoe groot de reticle size moet zijn
            if (ADS) {
                reticleSize = gunObj.spreadOnADS * reticleSizeMultiplier;
            } else {
                reticleSize = gunObj.spread * reticleSizeMultiplier;
            }


            // zorg dat de reticle niet te klein wordt
            if (reticleSize < minimumSize) {
                reticleSize = minimumSize;
            }


            // zet dit gelijk aan een x- of y-coördinaat
            if (moveComponent == Component.negativeHorizontal) transform.localPosition = new Vector3(-reticleSize, 0f, 0f);
            else if (moveComponent == Component.positiveHorizontal) transform.localPosition = new Vector3(reticleSize, 0f, 0f);
            else if (moveComponent == Component.negativeVertical) transform.localPosition = new Vector3(0f, -reticleSize, 0f);
            else if (moveComponent == Component.positiveVertical) transform.localPosition = new Vector3(0f, reticleSize, 0f);

            // UnityEngine.Debug.Log($"reticleSize: {reticleSize}, localPosition: {transform.localPosition}");
        }
    }
}
