using System.Collections;
using System.Collections.Generic;
using static BulletScript2;
using UnityEngine;

public class scope : MonoBehaviour
{
    public Animator animator;
    public GameObject scopeOverlay;
    public GameObject weaponCamera;
    public Camera mainCamera;
    public float scopedFOV = 15f;
    public BulletScript2 bulletScriptObj;
    private float spread;

    private bool isScoped = false;




    void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            isScoped = !isScoped;
            animator.SetBool("Scoped", isScoped);
            if (isScoped)
                StartCoroutine(onScoped());
            else
                onUnscoped();

        }

    }
    IEnumerator onScoped()
    {
        yield return new WaitForSeconds(.15f);
        bulletScriptObj.spread = 0;
        scopeOverlay.SetActive(true);
        weaponCamera.SetActive(false);
        
        mainCamera.fieldOfView = 15f;
    }
    void onUnscoped()
    {
        scopeOverlay.SetActive(false);
        weaponCamera.SetActive(true);
        mainCamera.fieldOfView = 60;
        bulletScriptObj.spread = 2;
    }






}

