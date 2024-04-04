using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [Header("Door Movement")]
    public Vector3 closedPosition;
    public Vector3 openedPosition;
    public float openCloseTime = 1f;
    public bool open = false;


    private float animationTime;


    void Awake() {
        // open the door if the default open bool is set to true, otherwise set it to false
        if (open) transform.localPosition = openedPosition;
        else transform.localPosition = closedPosition;
    }


    void Update() {
        // move the door if the door is opening
        if (animationTime < openCloseTime) {
            // UnityEngine.Debug.Log("Animating door");
            // bepaal de richting
            Vector3 moveDirection;
            if (open) {
                moveDirection = openedPosition - closedPosition;
                transform.localPosition = closedPosition + moveDirection * (animationTime/openCloseTime);
            }
            else {
                moveDirection = closedPosition - openedPosition;
                transform.localPosition = openedPosition + moveDirection * (animationTime/openCloseTime);
            }

            

        }
        else {
            // UnityEngine.Debug.Log("Stationary Door");
            // set the position of the door to open or close if the animation has ended
            if (open) transform.localPosition = openedPosition;
            else transform.localPosition = closedPosition;
        }


        animationTime += Time.deltaTime;
    }



    public void OpenDoor() {
        // UnityEngine.Debug.Log("Opening door");
        if (!open) {
            open = true;
            animationTime = 0f;
        }
    }
    
    public void CloseDoor() {
        // UnityEngine.Debug.Log("Closing door");
        if (open) {
            open = false;
            animationTime = 0f;
        }
    }
}
