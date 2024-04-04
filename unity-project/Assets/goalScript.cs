using System.Collections;
using System.Collections.Generic;
using static DoorScript;
using static NewEnemySpawnController;
using static PlayerMovement;
using UnityEngine;
using TMPro;

public class goalScript : MonoBehaviour
{

    [Header("Health settings")]
    public int maxHealth = 200;
    public int healthAddition;
    public int healthRemoval;
    public float goalDamageTimer;
    private float timePassed = 0f;
    public string objectType;

    [Header("TMPro")]
    public string countdownDisplayName;
    public string objectiveText;
    private TextMeshProUGUI countdownDisplay;

    private GameObject playerObj;
    private GameObject goalSpawner1;
    private GameObject goalSpawner2;

    [Header("Enemy Spawning")]
    public string enemySpawnerMasterName;
    public int newSpawnerIndex;

    [Header("Doors")]
    public bool enableDoor;
    public string doorName;
    private DoorScript doorObj;

    

    void Awake() {
        // playerObj = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }


    void Start()
    {
        countdownDisplay = GameObject.Find(countdownDisplayName).GetComponent<TextMeshProUGUI>();
        if (enableDoor) doorObj = GameObject.Find(doorName).GetComponent<DoorScript>();
        Invoke("Lowermaxhealth", goalDamageTimer);

        // close door
        if (enableDoor) doorObj.CloseDoor();
    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;

        float timeRemaining = goalDamageTimer - timePassed;
        if (countdownDisplay != null){
            countdownDisplay.SetText($"{objectiveText}: {(int)timeRemaining}s");
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) {
            if(objectType == "goal")
            {
                // Debug.Log("itworks");



                if(GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth < maxHealth)
                {
                    GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth += healthAddition;
                    if (GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth > maxHealth) GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth = maxHealth;
                    GameObject.Find("GoalSpawner")?.GetComponent<goalSpawner>()?.SpawnGoal();
                }
                else{
                    GameObject.Find("GoalSpawner")?.GetComponent<goalSpawner>()?.SpawnGoal();
                }
                
            }
            else if(objectType == "goal1")
            {
                // Debug.Log("itworks");




                if(GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth < maxHealth)
                {
                    GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth += healthAddition;
                    if (GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth > maxHealth) GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth = maxHealth;
                    GameObject.Find("GoalSpawner2")?.GetComponent<goalSpawner>()?.SpawnGoal();
                }
                else {
                    GameObject.Find("GoalSpawner2")?.GetComponent<goalSpawner>()?.SpawnGoal();
                }
                
            }

            if (countdownDisplay != null){
                countdownDisplay.SetText($"{objectiveText}: ** Clear **");
            }


            // change SubspawnerIndex of the enemy spawn controller
            GameObject.Find(enemySpawnerMasterName).GetComponent<NewEnemySpawnController>().activeSubspawnerIndex = newSpawnerIndex;


            // open door
            if (enableDoor) doorObj.OpenDoor();



            Destroy(gameObject);
        }
    }
    
    void Lowermaxhealth()
    {
        // Debug.Log("lowermaxhealth runs");


        GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth -= healthRemoval;
        if(objectType == "goal")
        {
            GameObject.Find("GoalSpawner")?.GetComponent<goalSpawner>()?.SpawnGoal();
        }
        else if(objectType == "goal1")
        {
            GameObject.Find("GoalSpawner2")?.GetComponent<goalSpawner>()?.SpawnGoal();
        }
        

        if (countdownDisplay != null){
            countdownDisplay.SetText($"{objectiveText}: ** Decayed **");
        }


        // change SubspawnerIndex of the enemy spawn controller
        GameObject.Find(enemySpawnerMasterName).GetComponent<NewEnemySpawnController>().activeSubspawnerIndex = newSpawnerIndex;

        // open door
        if (enableDoor) doorObj.OpenDoor();


        Destroy(gameObject);
    }
}
