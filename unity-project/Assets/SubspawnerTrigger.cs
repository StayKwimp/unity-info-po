using System.Collections;
using System.Collections.Generic;
using static NewEnemySpawnController;
using UnityEngine;

public class SubspawnerTrigger : MonoBehaviour
{
    [Header("Definitions")]
    public GameObject masterSpawnControllerObj;
    public GameObject playerObj;

    [Header("Change index")]
    public int subspawnerIndexToChangeTo;

    [Header("Trigger distance")]
    public float triggerDistance = 2f;


    void Awake() {
        // check if the gameobject variables are defined
        if (masterSpawnControllerObj == null || playerObj == null) {
            UnityEngine.Debug.LogError($"masterSpawnControllerObj or playerObj are undefined on trigger object {gameObject.name}, destroying the object.");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        var toPlayerVector = transform.position - playerObj.transform.position;
        var distanceToPlayer = toPlayerVector.magnitude;
        
        // if the player is close enough to the trigger, change subspawnerIndex of the master spawn controller
        if (distanceToPlayer <= triggerDistance) {
            masterSpawnControllerObj.GetComponent<NewEnemySpawnController>().activeSubspawnerIndex = subspawnerIndexToChangeTo;
        }
    }




    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}
