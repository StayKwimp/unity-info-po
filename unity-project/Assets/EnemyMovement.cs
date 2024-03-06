using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
using static EnemyMovement;
using static AudioManager;
using System;
using System.Numerics;


public class EnemyMovement : MonoBehaviour
{
    [Header("Definitions")]
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public int health;
    


    private bool shotByPlayer;
    private UnityEngine.Vector3 playerPosition;


    // patrolling state
    [Header("Patrolling State")]
    public UnityEngine.Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;
    public int maxWalkPointReachTime;
    private float walkPointReachTimer = 0;

    
    // attacking state
    [Header("Attacking State")]
    public float timeBetweenAttacks;
    public enum Weapon {Bullets, Grenades};
    public Weapon attackType;
    private bool alreadyAttacked = false;
    public float ChaseDelay = 1;

    [Header("Grenades/Bullets")]
    public GameObject projectile;
    public float[] shootForceMul = {1, 1, 1};
    public Transform bulletSpawnpoint;
    public UnityEngine.Vector3 targetingPointOffset; // de y component moet negatief zijn als je wil dat hij omhoog richt en positief als je wil dat hij naar beneden richt.

    private UnityEngine.Vector3 bulletOrigin;

    [Header("Bullets")]
    public float spread; 
    public float bulletShootForceMul;
    public float[] playerMovementAdjustment = {1, 1};
    public GameObject muzzleFlash;
    public float bulletMaxTime;


    // states
    [Header("Sight/Attack Range")]
    public float leaveAttackingStateRange;
    public float sightRange, hearingRange, attackRange;
    public bool playerInSightRange, playerInHearingRange, playerInAttackRange, playerInLeaveAttackRange;
    private bool attacking = false;

    [Header("Reticle Coloring")]
    public GameObject reticleGameObject;

    [Header("Healing upon death")]
    public int healthBoost;


    private AudioManager audioManager;
    

    private void Awake() {
        // vind de speler
        player = GameObject.Find("PlayerObj").transform;
        agent = GetComponent<NavMeshAgent>();

        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }


    void Update()
    {
        // check of de speler in de sight en attack range is
        playerInLeaveAttackRange = Physics.CheckSphere(transform.position, leaveAttackingStateRange, whatIsPlayer);
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        // playerInHearingRange = Physics.CheckSphere(transform.position, hearingRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // UnityEngine.Debug.Log($"playerInSightRange: {playerInSightRange}, playerInHearingRange: {playerInHearingRange}, playerInAttackRange: {playerInAttackRange}, playerInLeaveAttackRange: {playerInLeaveAttackRange}, attacking: {attacking}");

        RaycastHit hit;
        bool hittable;
        // doe een raycast om te controleren of de speler daadwerkelijk in de line of sight van de mogus zit

        UnityEngine.Vector3 directionToPlayer = player.position - bulletSpawnpoint.position;

        if(Physics.Raycast(bulletSpawnpoint.position, directionToPlayer, out hit, sightRange))
        {
            // Debug.Log(hit.collider);
            hittable = hit.collider.CompareTag("Player");

            // verander alleen als playerInSightRange al true is
            // controleert of de mogus de player wel kan zien
            if (playerInSightRange) playerInSightRange = hit.collider.CompareTag("Player");

        }
        else {
            hittable = false;
            playerInSightRange = false;
            // Debug.Log("Raycast hit nothing");
        }









        if (attacking) {
            attacking = playerInLeaveAttackRange;
            if (!hittable) attacking = false;  
        }

        if (!playerInSightRange && !shotByPlayer) {
            Patrolling();
            attacking = false;
        }
        else if (!playerInAttackRange && !playerInLeaveAttackRange && (playerInSightRange)) {
            ChasePlayer();
        }

        if ((playerInAttackRange && playerInSightRange && hittable)|| attacking) {
            AttackPlayer();
            attacking = true;
        }
        else if (playerInAttackRange && (playerInSightRange) && !hittable)
        {   
            ChasePlayer();
        }


        if (shotByPlayer) {
            // UnityEngine.Debug.Log("Going towards player shot position");
            GotoShotDirection();
        }
        
    }



    // state functions


    private void Patrolling() {
        // UnityEngine.Debug.Log("Patrolling");
        // als er geen punt is waar de enemy heen moet lopen, zoek er dan naar eentje
        if (!walkPointSet) {
            SearchForWalkPoint();
            walkPointReachTimer = maxWalkPointReachTime;
        }

        // als dat wel zo is, loop er heen
        else agent.SetDestination(walkPoint);


        // controleer of je bij de destination bent aangekomen
        UnityEngine.Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f) walkPointSet = false;



        // als de enemy niet binnen een bepaalde tijd bij de walkpoint is, zoek dan een nieuwe walkpoint
        walkPointReachTimer -= Time.deltaTime;
        if (walkPointReachTimer <= 0) walkPointSet = false;
    }



    private void GotoShotDirection() {
        // UnityEngine.Debug.Log("GotoShotDirection");
        agent.SetDestination(playerPosition);

        // controleer of de mogus al bij de positie is aangekomen
        UnityEngine.Vector3 distanceToWalkPoint = transform.position - playerPosition;

        // zo ja, dan wordt shotByPlayer uitgezet
        if (distanceToWalkPoint.magnitude < 1f) {
            // UnityEngine.Debug.LogWarning("Canceled shotByPlayer at GotoShotDirection (mogus reached its end point)");
            shotByPlayer = false;
        }
    }


    // deze functie wordt uitgevoerd in PlayerMovement, PlayerGun, PlayerGrenade, MogusExplosiveBullet en EnemyMovement
    // het zorgt dat enemies op geluiden van geweerschoten en granaten af gaan
    public void HearedPlayer(UnityEngine.Vector3 gotoPosition) {
        UnityEngine.Debug.Log("HearedPlayer");

        agent.SetDestination(gotoPosition);
        walkPoint = gotoPosition;
        walkPointSet = true;
        walkPointReachTimer = 60f;
    }


    private void SearchForWalkPoint() {
        // UnityEngine.Debug.Log("SearchForWalkPoint");
        // neem een willekeurige plek binnen een bepaalde range om heen te lopen
        float randomZ = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
        float randomX = UnityEngine.Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new UnityEngine.Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        // controleer of walkpoint binnen de map is
        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround)) walkPointSet = true;
    }




    private void ChasePlayer() {
        // UnityEngine.Debug.Log("ChasePlayer");
        // if (shotByPlayer) UnityEngine.Debug.LogWarning("Cancelled shotByPlayer at ChasePlayer");
        shotByPlayer = false;
        // loop naar de speler toe
        agent.SetDestination(player.position);

        // voor als de mogus weer naar de patrolling state gaat
        walkPoint = player.position;
        walkPointSet = true;
        walkPointReachTimer = 60f;
    }



    private IEnumerator DelayChase()
    {
        //zorgt ervoor dat het 1 seconde duurt voordat hij tot stilstand komt
        yield return new WaitForSeconds(ChaseDelay);

        // zorg dat de enemy niet gaat gebewegen door de destination op zijn current position te zetten
        agent.SetDestination(transform.position);

    }






    private void AttackPlayer() {
        // UnityEngine.Debug.Log("AttackPlayer");
        // if (shotByPlayer) UnityEngine.Debug.LogWarning("Cancelled shotByPlayer at AttackPlayer");
        shotByPlayer = false;

        StartCoroutine(DelayChase());

        

        // we willen niet dat de enemy model naar boven of beneden draait. daarom laten we de y rotation staan op die van de enemy zelf
        // LookAt neemt de twee verschillende positions van de Vector3 en de transform,
        // en returnt daartussen een rotation waarop de enemy naar de speler kijkt.
        // dit doen we niet voor de y rotation (want die is in playerXZPos en transform hetzelfde, dus nul)
        var playerXZPos = new UnityEngine.Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(playerXZPos);


        if (!alreadyAttacked) {
            bulletOrigin = bulletSpawnpoint.position;


            if (attackType == Weapon.Grenades) AttackPlayerWithGrenade();
            else if (attackType == Weapon.Bullets) AttackPlayerWithBullet();

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }


        // voor als de mogus weer naar de patrolling state gaat
        walkPoint = player.position;
        walkPointSet = true;
        walkPointReachTimer = 60;
    }


    private void AttackPlayerWithGrenade() {
        
        // shoot les granades (amazingk)
        //hier wordt transform.TransformDirection gebruikt om de positie van de bulletOrigin relatief ten op zichte van de enemy te houden
        // Vector3 bulletOriginPosition = transform.position + transform.TransformDirection(bulletOrigin);

        var rb = Instantiate(projectile, bulletOrigin, transform.rotation).GetComponent<Rigidbody>();
        UnityEngine.Vector3 distanceToPlayer = player.position - bulletOrigin;
        UnityEngine.Vector3 shootForce = new UnityEngine.Vector3(distanceToPlayer.x * shootForceMul[0], distanceToPlayer.y * shootForceMul[1] + 5f, distanceToPlayer.z * shootForceMul[2]);



        //hier wordt transform.TransformDirection gebruikt om de vector relatief ten op zichte van de shootForce vector te houden
        rb.AddForce(shootForce - transform.TransformDirection(targetingPointOffset), ForceMode.Impulse);        
    }



    private void AttackPlayerWithBullet() {
        // eerst de player velocity bepalen, en dan nog de movement van de player voorspellen en die erbij doen

        var playerVelocity = player.GetComponentInParent<Rigidbody>().velocity;
        var distanceToPlayer = player.position - bulletOrigin;
        
        // afstand maal een constante wordt de aiming position (een rare vorm van pythagoras i guess)
        var multiplierXZ = distanceToPlayer.magnitude * playerMovementAdjustment[0];
        var multiplierY = distanceToPlayer.magnitude * playerMovementAdjustment[1];

        // Debug.Log(distanceToPlayer.magnitude);
        
        var aimingPosition = player.position + targetingPointOffset + new UnityEngine.Vector3(playerVelocity.x * multiplierXZ, playerVelocity.y * multiplierY, playerVelocity.z * multiplierXZ);
        // var aimingPosition = player.position + targetingPointOffset + new UnityEngine.Vector3(playerVelocity.x * playerMovementAdjustment[0], playerVelocity.y * playerMovementAdjustment[1], playerVelocity.z * playerMovementAdjustment[0]);

        // vector AB = position B - position A
        UnityEngine.Vector3 directionWithoutSpread = aimingPosition - bulletOrigin;


        UnityEngine.Debug.DrawLine(bulletOrigin, aimingPosition, Color.green, 2f);

        

        // bullet spread toevoegen
        float xSpread = UnityEngine.Random.Range(-spread, spread);
        float ySpread = UnityEngine.Random.Range(-spread, spread);


        UnityEngine.Vector3 directionWithSpread = directionWithoutSpread + new UnityEngine.Vector3(xSpread, ySpread, 0f);


        // bullet spawnen
        GameObject currentBullet = Instantiate(projectile, bulletOrigin, UnityEngine.Quaternion.identity);
        
        // draai de bullet
        currentBullet.transform.forward = directionWithSpread.normalized;

        // zorg dat de bullet daadwerkelijk ergens heen vliegt
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * bulletShootForceMul, ForceMode.Impulse);


        // muzzle flash
        if (muzzleFlash != null) {
            Instantiate(muzzleFlash, bulletOrigin, UnityEngine.Quaternion.identity);
        }



        alreadyAttacked = true;
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }







    private void ResetAttack() {
        alreadyAttacked = false;
    }
    

    public void TakeDamage(int damage) {
        
        health -= damage;
        if (health <= 0) Invoke("DestroyEnemy", 0.05f);

        // zorg dat de mogus in de richting gaat van waar hij beschoten was
        shotByPlayer = true;
        playerPosition = player.position;

        
        // speelt als het goed is geluid af
        // audioManager.Play("bullet hit");
        // UnityEngine.Debug.LogWarning("Enemy is hit!");
    }

    private void DestroyEnemy() {
        if (GameObject.Find("Player").GetComponent<PlayerMovement>().health <= GameObject.Find("Player").GetComponent<PlayerMovement>().maxHealth)
        {
            GameObject.Find("Player").GetComponent<PlayerMovement>().health += healthBoost;
        }
        Destroy(gameObject);

        // voeg een kill toe
        GameObject.Find("Player").GetComponent<PlayerMovement>().kills += 1;

        // flash de reticle
        reticleGameObject.GetComponent<ReticleColor>().FlashReticleColor();
    }
}