using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class goalSpawner : MonoBehaviour
{

    [Header("Spawnpoints")]
    public GameObject spawnerObject;
    public float respawnDelay;
    // if spawnGoalOnGameStart is false, a goal will be spawned after respawnDelay amount of seconds
    public bool spawnGoalOnGameStart;

    // Start is called before the first frame update
    void Start()
    {
        if (spawnGoalOnGameStart) SpawnObject();
        else SpawnGoal();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SpawnGoal()
    {
        //ik weet dat dit heel erg lelijk is.
        // Debug.Log("spawngoal works");
        Invoke("SpawnObject", respawnDelay);
    }

    private void SpawnObject()
    {
        // Debug.Log("spawn new object works");
        Instantiate(spawnerObject, transform.position, Quaternion.identity);
    }

}