using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class goalScript : MonoBehaviour
{

    [Header("Health settings")]
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

    

    void Awake() {
        // playerObj = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }


    void Start()
    {
        countdownDisplay = GameObject.Find(countdownDisplayName).GetComponent<TextMeshProUGUI>();
        Invoke("Lowermaxhealth", goalDamageTimer);
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
                Debug.Log("itworks");
                if(GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth < 200)
                {
                    GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth += healthAddition;
                    GameObject.Find("GoalSpawner")?.GetComponent<goalSpawner>()?.SpawnGoal();
                }
                else{
                    GameObject.Find("GoalSpawner")?.GetComponent<goalSpawner>()?.SpawnGoal();
                }
                
            }
            else if(objectType == "goal1")
            {
                Debug.Log("itworks");
                if(GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth < 200)
                {
                    GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth += healthAddition;
                    GameObject.Find("GoalSpawner2")?.GetComponent<goalSpawner>()?.SpawnGoal();
                }
                else {
                    GameObject.Find("GoalSpawner2")?.GetComponent<goalSpawner>()?.SpawnGoal();
                }
                
            }

            if (countdownDisplay != null){
                countdownDisplay.SetText($"{objectiveText}: ** Clear **");
            }
            Destroy(gameObject);
        }
    }
    
    void Lowermaxhealth()
    {
        Debug.Log("lowermaxhealth runs");
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
        Destroy(gameObject);
    }
}
