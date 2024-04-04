using System.Collections;
using System.Collections.Generic;
using static DoorScript;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("Door")]
    public GameObject doorObj;
    public ChangeDoorState newDoorStateOnTrigger;
    public enum ChangeDoorState {
        open,
        close,
        flip
    }

    [Header("Player")]
    public GameObject playerObj;

    [Header("Trigger Distance")]
    public float triggerDistance = 2f;

    void Awake()
    {
        // check if the gameobject variables are defined
        if (doorObj == null || playerObj == null) {
            UnityEngine.Debug.LogError($"doorObj or playerObj are undefined on trigger object {gameObject.name}, destroying the object.");
            Destroy(gameObject);
        }
    }

    
    void Update()
    {
        var toPlayerVector = transform.position - playerObj.transform.position;
        var distanceToPlayer = toPlayerVector.magnitude;
        
        // if the player is close enough to the trigger, open or close the door
        if (distanceToPlayer <= triggerDistance) {
            if (newDoorStateOnTrigger == ChangeDoorState.open)
                doorObj.GetComponent<DoorScript>().OpenDoor();

            else if (newDoorStateOnTrigger == ChangeDoorState.close)
                doorObj.GetComponent<DoorScript>().CloseDoor();

            else if (newDoorStateOnTrigger == ChangeDoorState.flip) {
                // flip door state
                if (doorObj.GetComponent<DoorScript>().open)
                    doorObj.GetComponent<DoorScript>().CloseDoor();
                
                else
                    doorObj.GetComponent<DoorScript>().OpenDoor();
            }
               

        }
    }


    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}
