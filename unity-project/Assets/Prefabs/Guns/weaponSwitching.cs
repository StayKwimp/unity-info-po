using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponSwitching : MonoBehaviour
{
    public int selectedWeapon = 0;
    public bool switchWeaponsOnMouseScroll;


    private void Start()
    {
        SelectWeapon();
    }
    private void Update()
    {
        int prevSelectedWeapon = selectedWeapon;

        // switch van wapen als je scrolt
        if (switchWeaponsOnMouseScroll) {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                if (selectedWeapon >= transform.childCount - 1)
                    selectedWeapon = 0;
                else
                    selectedWeapon++;
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                if (selectedWeapon <= 0)
                    selectedWeapon = transform.childCount - 1;
                else
                    selectedWeapon--;
            }
        }


        


        // switch van wapen met de toetsen 1, 2 en 3
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedWeapon = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedWeapon = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedWeapon = 2;
        }


        // switch van wapen als dat gebeurd is
        if (prevSelectedWeapon != selectedWeapon)
        {
            SelectWeapon();
        }
    }



    void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
                weapon.gameObject.SetActive(true);
            else
                weapon.gameObject.SetActive(false);
            
            
            i++;
        }
    }

}
