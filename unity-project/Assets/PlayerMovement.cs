using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using static PlayerGun;

public class PlayerMovement : MonoBehaviour
{
    // hmm yes bewegen my beloved
    [Header("Definitions")]
    public Camera playerCam;

    [Header("Movement")]
    private float mvSpeed;
    private float maxSpeed;
    public float walkSpeed;
    public float maxWalkSpeed;
    public float sprintSpeed;
    public float maxSprintSpeed;
    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public int maxJumps;
    public int jumpsLeft = 0;
    bool readyToJump = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float maxCrouchSpeed;
    public float crouchYScale;
    public bool toggleCrouch;
    private float startYScale;

    [Header("ADS")]
    public float movementSpeedMultiplierOnADS;
    public PlayerGun playerGunScr;

    // key bindings
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode grenadeKey;
    private bool crouchKeyPressed;

    // controle of de speler op de grond staat of niet
    [Header("Ground Check")]
    public float playerHeight;
    public float playerWidth;
    public LayerMask whatIsGround;
    bool grounded;


    // het handelen van slopes
    [Header("Slope Handling")]
    public float maxSlopeAngle;
    public float slopeSpeedMultiplier;
    public float maxSpeedOnSlopeMultiplier;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Wall Handling")]
    private RaycastHit wallHit;
    private Vector3 wallVector = Vector3.zero;


    [Header("Player Stats")]
    public int health;
    public int maxHealth;
    public int armor;
    public int maxArmor;
    public float healCooldownTime;
    public int healthPerSecond;
    public int grenades;

    [Header("Grenades")]
    public GameObject grenadeObj;
    public Vector3 grenadeSpawnpointDelta;
    public float throwForce;
    public float fuseLength = 4f;
    public bool throwingGrenade;
    // zet de grenade animation progress op een groot getal om te voorkomen dat er animaties gaan spelen aan het begin van de game (want anders is de animation progress gelijk aan nul, en gaat de gun zichzelf animaten)
    public float grenadeAnimationProgress;
    private bool thrownGrenade;
    private float throwGrenadeAtAnimationTime = 0.5f;
    // public want PlayerGun heeft het ook nodig
    public float animationStopTime = 0.6f;

    // health
    private float healthBuffer;
    private float secondsPastTakingDamage;

    [Header("Graphics")]
    public TextMeshProUGUI healthDisplay;
    public TextMeshProUGUI killsDisplay;
    public int kills = 0;





    public Transform orientation;

    float currentSpeed;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;


    // player state machine
    public movementState state;
    public enum movementState {
        walking,
        sprinting,
        crouching,
        air
    }

   

    private void Awake() {
        // zorg dat de player niet omvalt (rotation freezen)
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // verander StartYScale
        startYScale = transform.localScale.y;

        // zet de grenade animation progress nogmaals op een groot getal
        grenadeAnimationProgress = 300f;
        int jumpsLeft = maxJumps;
    }


    private void Update() {
        health = 20000;
        // Debug.Log($"GrenadeAnimationProgress: {grenadeAnimationProgress}");
        // ground check d.m.v. een raycast naar beneden van de helft van de spelerhoogte plus 0.2
        // de layermask whatIsGround wordt gebruikt in unity om te kijken welke objects allemaal als
        // grond gezien worden en welke niet
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);


        MyInput();
        SpeedControl();
        StateHandler();
        HealPlayer();

        GrenadeHandler();

        // zorg dat de drag nul is als de player niet op de grond staat
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
        

        // update de health bar display als het bestaat
        if (healthDisplay != null) healthDisplay.SetText(string.Format("Health: {0} ({1})", health, maxHealth));

        // net zoals de kills display
        if (killsDisplay != null) killsDisplay.SetText(string.Format("Kills: {0}", kills));


        // check of de speler onder y = -64 is.
        // als dat zo is, e l i m i n e e r de speler
        if (gameObject.transform.position.y <= -64f) {
            TakeDamage(maxHealth + 1);
        }
    }


    private void GrenadeHandler() {
        if (throwingGrenade) {
            // gooi pas een granaat na 0.5s
            if (grenadeAnimationProgress >= throwGrenadeAtAnimationTime && !thrownGrenade) {
                // spawn grenade obj
                var grenadeSpawnpoint = transform.position + grenadeSpawnpointDelta;
                var spawnedGrenade = Instantiate(grenadeObj, grenadeSpawnpoint, Quaternion.identity);

                // bepaal de throw direction
                // dit doen we door een ray te maken die recht uit de camera gaat in de richting van waar je door de camera kijkt.
                // 0.5f en 0.5f is het midden van het scherm
                // daarna gebruiken we de property .direction van een Ray (viewportPointToRay returnt een Ray)
                Vector3 throwDirection = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)).direction;

                // rotate de g r a n a d u s
                spawnedGrenade.transform.forward = throwDirection.normalized;

                // voeg krachten toe aan de granaedae
                spawnedGrenade.GetComponent<Rigidbody>().AddForce(throwDirection.normalized * throwForce, ForceMode.Impulse);

                // start de g r a n a e d a e fuse
                spawnedGrenade.GetComponent<PlayerGrenade>().StartFuse(fuseLength);


                thrownGrenade = true;
                grenades--;
            }

            // stop de animatie na 0.6s
            if (grenadeAnimationProgress >= animationStopTime) {
                throwingGrenade = false;
            }

        }
        grenadeAnimationProgress += Time.deltaTime;
    }


    private void HealPlayer() {
        // heal de player na zoveel seconden geen damage genomen te hebben
        // secondsPastTakingDamage wordt gereset in DamagePlayer()
        secondsPastTakingDamage += Time.deltaTime;

        if (secondsPastTakingDamage >= healCooldownTime) {
            // we gebruiken hier een health buffer, want health is een integer en je wilt hier een float hebben als buffer (anders heal je te snel)
            healthBuffer += healthPerSecond * Time.deltaTime;

            // voeg health toe als de buffer groter is dan 1
            if (healthBuffer >= 1f) {
                var healthIncrease = (int)healthBuffer;
                // (int) rondt altijd naar beneden af
                health += healthIncrease;

                healthBuffer -= healthIncrease;
            }
            

            // zorg dat de player niet meer health kan healen dan de max
            if (health > maxHealth) health = maxHealth;
        }
    }

    private void FixedUpdate() {
        MovePlayer();

        // get current movement speed
        currentSpeed = rb.velocity.magnitude;
    }



    private void MyInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");


        // controleer of de speler op de springknop drukt
        if (Input.GetKeyDown(jumpKey)) {
            //zet readyToJump naar true of false afhankelijk van de omstandigheden
            CheckJump();
            
            if(readyToJump){
                Jump();
            }

        }


        // voor als toggleCrouch wel aan staat, zie de code in StateHandler()
        if (!toggleCrouch) {
            // start met crouchen
            if (Input.GetKeyDown(crouchKey)) {
                // maak de speler kleiner qua hoogte
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

                // als je de speler kleiner maakt op de Y-as zorgt het er ook voor dat ie in de lucht gaat vliegen
                // daarom moeten we een kracht naar beneden maken die één keer wordt geactiveerd
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }


            // stop met crouchen
            if (Input.GetKeyUp(crouchKey)) {
                // maak de speler weer normaal
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
        }

        // granador
        var reloadingGun = playerGunScr.reloading;
        if (Input.GetKeyDown(grenadeKey) && grounded && !throwingGrenade && !reloadingGun) {
            throwingGrenade = true;
            grenadeAnimationProgress = 0f;
            thrownGrenade = false;
        }
    }



    private void StateHandler() {
        // switch naar de crouching state als de crouch key ingedrukt is
        // als togglecrouch true is ga je crouchen als je de crouch key indrukt en weer stoppen met crouchen
        // als de crouch key weer opnieuw ingedrukt is (dus voor een tweede keer)


        // als togglecrouch false is ga je alleen crouchen als je de crouch key ingedrukt hebt
        if (toggleCrouch && grounded && Input.GetKeyDown(crouchKey)) {



            if (state == movementState.crouching) {
                // stop met crouchen
                state = movementState.walking;
                
                // maak de speler weer normaal
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                // playerGunTransform.localScale = new Vector3(playerGunTransform.localScale.x, playerGunTransform.localScale.y, gunStartZScale);

            }
            else {
                // begin met crouchen
                state = movementState.crouching;

                // maak de speler kleiner qua hoogte
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                // playerGunTransform.localScale = new Vector3(playerGunTransform.localScale.x, playerGunTransform.localScale.y, gunCrouchZScale);
                
                

                // als je de speler kleiner maakt op de Y-as zorgt het er ook voor dat ie in de lucht gaat vliegen
                // daarom moeten we een kracht naar beneden maken die één keer wordt geactiveerd
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

                
            }




        } else if (grounded && !toggleCrouch) {
            // zet de state op walking, maar als de crouch key ingedrukt is, zet hem weer op crouching
            state = movementState.crouching;
            if (Input.GetKey(crouchKey)) state = movementState.crouching;
        }

        

        // switch naar sprinting state als je op de grond staat en sprint ingedrukt is
        if (grounded && Input.GetKey(sprintKey) && state != movementState.crouching) {
            state = movementState.sprinting;
            mvSpeed = sprintSpeed;
            maxSpeed = maxSprintSpeed;
        }


        // switch naar walking state als je op de grond staat en sprint niet ingedrukt is
        else if (grounded && state != movementState.crouching) {
            state = movementState.walking;
            mvSpeed = walkSpeed;
            maxSpeed = maxWalkSpeed;
        }


        // als je uiteindelijk niet op de grond staat, switch dan naar de air state (en stop met crouchen)
        else if (!grounded) {
            if (state == movementState.crouching) {
                // undo de crouch
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
            state = movementState.air;
        }



        if (state == movementState.crouching) {
            state = movementState.crouching;
            mvSpeed = crouchSpeed;
            maxSpeed = maxCrouchSpeed;
        }
    }





    private void MovePlayer() {

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        //Debug.Log(wallVector);
        
        Vector3 moveDirectionNormalized = Vector3.zero;

        if (Mathf.Abs(wallVector.x) > Mathf.Abs(moveDirection.normalized.x)) moveDirectionNormalized.x = 0f;
        else moveDirectionNormalized.x = moveDirection.normalized.x + wallVector.x;

        if (Mathf.Abs(wallVector.z) > Mathf.Abs(moveDirection.normalized.z)) moveDirectionNormalized.z = 0f;
        else moveDirectionNormalized.z = moveDirection.normalized.z + wallVector.z;
        
        moveDirectionNormalized.y = moveDirection.normalized.y + wallVector.y;


        
        if (Mathf.Sign(moveDirection.normalized.x) == Mathf.Sign(wallVector.x)) moveDirectionNormalized.x = moveDirection.normalized.x;
        if (Mathf.Sign(moveDirection.normalized.z) == Mathf.Sign(wallVector.z)) moveDirectionNormalized.z = moveDirection.normalized.z;
        

        //Debug.Log("moveDirection: " + moveDirection.ToString() + ", normalised: " + moveDirection.normalized.ToString() + ", result: " + moveDirectionNormalized.ToString());

        wallVector = Vector3.zero;


        // moving on a slope
        if (OnSlope() && !exitingSlope) {
            rb.AddForce(GetSlopeMoveDirection() * mvSpeed * slopeSpeedMultiplier, ForceMode.Force);


            // voeg een kracht omlaag toe, zodat de speler op de slope blijft
            // dit voorkomt ook gelijk dat de speler in de lucht vliegt als het de slope af loopt.
            if (rb.velocity.y != 0) 
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            
            
        }


        // disable gravity when on a slope (zodat je er niet vanaf valt)
        // dit is geen probleem, want als je van de slope af gaat wordt de zwaartekracht
        // weer aangezet
        rb.useGravity = !OnSlope();


        if (grounded) {
            // forcemode.force zorgt ervoor dat de kracht de hele tijd toegepast kan worden
            rb.AddForce(moveDirectionNormalized * mvSpeed * 10f, ForceMode.Force);
        } else {
            // in de lucht worden krachten met airMultiplier vermenigvuldigd, wat ervoor kan
            // zorgen dat je langzamer je direction kan aanpassen (als airMultiplier < 1)
            rb.AddForce(moveDirectionNormalized * mvSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }



    private void SpeedControl() {
        // limit speed on slope
        if (OnSlope() && !exitingSlope) {
            if (rb.velocity.magnitude > maxSpeed * maxSpeedOnSlopeMultiplier)
                rb.velocity = rb.velocity.normalized * maxSpeed * maxSpeedOnSlopeMultiplier;
            

        // limit speed on ground/air
        } else {
            

            var ADSEnabled = playerGunScr.ADSEnabled;

            // als ADS aan is, is de maxSpeed kleiner
            if (ADSEnabled) {
                LimitMaxSpeed(maxSpeed * movementSpeedMultiplierOnADS);

            } else {
                LimitMaxSpeed(maxSpeed);
            }
        }
    }

    private void LimitMaxSpeed(float maximumSpeed) {
        // get current horizontal velocity
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        
        // if current velocity is higher than the maximum allowed movement speed
        if (flatVel.magnitude > maximumSpeed) {
            // calculate maximum allowed velocity (by max speed)
            Vector3 limitedVelocity = flatVel.normalized * maximumSpeed;

            // apply new limited velocity to player object
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }



    private void Jump() {
        jumpsLeft -= 1;
        exitingSlope = true;
        // reset y velocity
        // hierdoor spring je altijd even hoog
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // voeg een nieuwe kracht toe met forcemode op impulse, wat ervoor zorgt dat deze kracht
        // maar een keer wordt toegepast
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        
        
    }

    private void CheckJump() {

    
        //checkt of de speler op de grond staat
        if(grounded)
        {
            jumpsLeft = maxJumps;
            readyToJump = true;
            exitingSlope = false;
        }
        if(jumpsLeft > 0)
        {
            readyToJump = true;
            exitingSlope = false;
        }
        else{
            readyToJump = false;
            exitingSlope = false;
        }
    }



    public void OnCollisionStay(Collision collision) {
        // als de collider de tag Wall heeft
        if (collision.collider.CompareTag("Wall")) {
            // get surface of collider with a raycast

            Transform wallTransform = collision.transform;
            Vector3 raycastDirection = new Vector3(wallTransform.position.x - transform.position.x, 0f, wallTransform.position.z - transform.position.z);


            Physics.Raycast(transform.position, raycastDirection, out wallHit, raycastDirection.magnitude);
            wallVector = wallHit.normal;
            wallVector.y = 0f;
            


            // als de raycast de boven of onderkant van het object raakt, dan is wallVector nul dus dan moet de origin van de raycast
            // naar beneden of boven om te zorgen dat het de zijkant raakt
            float playerHeightMul = -0.5f;

            while (wallVector == Vector3.zero) {

                Vector3 rayStartPos = new Vector3(transform.position.x, transform.position.y - playerHeight * playerHeightMul, transform.position.z);
                raycastDirection = new Vector3(wallTransform.position.x - rayStartPos.x, 0f, wallTransform.position.z - rayStartPos.z);
                

                Physics.Raycast(rayStartPos, raycastDirection, out wallHit, raycastDirection.magnitude);
                wallVector = wallHit.normal;
                wallVector.y = 0f;

                playerHeightMul += 0.1f;
                // zorg dat het spel niet vast loopt lol
                if (playerHeightMul > 0.5f) break;
            }

        }
    }



    private bool OnSlope() {
        // hier weer zoals eerst, een raycast van half spelerlengte plus 0.3, maar dan voor slopes
        // er wordt in plaats van een layermask een raycasthit variable gebruikt
        // de object die de raycast raakt wordt opgeslagen in slopeHit
        // dit omdat slopes verschillende angles kunnen hebben, dus we willen degene waarop we staan pinpointen
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)) {

            // meet de hoek tussen de normaal van de slope en een vector omhoog
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);

            // return true (staat er niet bij maar dat doet het wel)
            //als de angle kleiner is als de maxSlopeAngle en angle niet nul is
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }


    private Vector3 GetSlopeMoveDirection() {
        // hiermee wordt de move direction parallel gezet aan de slope angle
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }



    public void TakeDamage(int damage) {
        health -= damage;
        secondsPastTakingDamage = 0f;

        if (health <= 0) {
            // vul hier iets van een game over in ofzo
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);

            // uncomment dit hieronder en comment dit hierboven voor full releases!

            SceneManager.LoadScene("Deathmenu");
        }
    }
}
