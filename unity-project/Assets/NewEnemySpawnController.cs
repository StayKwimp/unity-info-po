using System.Collections;
using System.Collections.Generic;
using static SubSpawnController;
using UnityEngine;
using System;

public class NewEnemySpawnController : MonoBehaviour
{
    [Header("Sub-Spawncontrollers")]
    public GameObject[] subSpawnControllers;
    public int activeSubspawnerIndex = 0;

    
    void Update()
    {
        // crash niet als de array out of bounds is
        try {
            // zorg dat je ook negatieve indexes kan gebruiken om helemaal geen enemies te spawnen
            if (activeSubspawnerIndex >= 0) subSpawnControllers[activeSubspawnerIndex].GetComponent<SubSpawnController>().Activate();

            // for every sub-spawncontroller that isn't on activeSubspawnerIndex, change active to false
            for (int i = 0; i < subSpawnControllers.Length; i++) {
                if (i != activeSubspawnerIndex) { 
                    subSpawnControllers[i].GetComponent<SubSpawnController>().Deactivate();
                    UnityEngine.Debug.Log($"Changed spawner index {i} to inactive");
                }
            }
        

        } catch (Exception e) {
            UnityEngine.Debug.LogError($"Exception {e} in {gameObject.name}. Active Subspawner Index: {activeSubspawnerIndex}");
            // als de array out of bounds is, zet dan elke spawner op inactief
            for (int i = 0; i < subSpawnControllers.Length; i++) {
                subSpawnControllers[i].GetComponent<SubSpawnController>().Deactivate();
                UnityEngine.Debug.Log($"Changed spawner index {i} to inactive");
            }
        }
    }
}
