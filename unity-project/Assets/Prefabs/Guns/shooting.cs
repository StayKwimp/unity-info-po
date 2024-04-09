using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shooting : MonoBehaviour
{
    public Transform bulletSpawnPoint;
    public GameObject bulletPreFab;
    public float bulletSpeed = 10;


    void update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var bullet = Instantiate(bulletPreFab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bulletSpeed;
        }
    }
}
