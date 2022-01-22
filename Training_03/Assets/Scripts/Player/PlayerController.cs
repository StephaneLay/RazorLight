using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using EZCameraShake;


public class PlayerController : MonoBehaviour
{
    [Space]
    [Header("Game Stats")]
    [Space]
    public int maxBulletBounces;
    public int currenthealth;
    private int maxHealth = 3;
    

    [Space]
    [Header("Components")]
    [Space]

    public GameObject leftShipSection;
    public GameObject rightShipSection;
    public GameObject bulletPrefab;
    public Transform axialFirePoint;
    public bool isLeftFireEnabled;
    public Transform leftFirePoint;
    public bool isRightFireEnabled;
    public Transform rightFirePoint;
    public bool isGameRunning;

    public GameObject ship;

    private Rigidbody rb;
    private Camera mainCamera;
    PlayerInput playerInput;
    Rumble rumble;

    [Space]
    [Header("Particles")]
    [Space]

    public Material glowMaterial;
    public Vector2 lifetime;

    [Space]
    [Header("Controls")]
    [Space]

    public bool hasControl;
    public float moveSpeed;
    private Vector3 moveDirection;
    Vector3 aimDirection;
    Vector3 stickAim;

    [Space]
    [Header("Logic")]
    [Space]
    public bool isShooting;
    public bool isPause;

    [Space]
    [Header("Audio")]
    [Space]

    public AudioSource AS_fire;
    public AudioSource AS_hit;





    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = GameManager.gm.mainCamera;
        playerInput = GetComponent<PlayerInput>();
        currenthealth = maxHealth;
        rumble = GetComponent<Rumble>();
        
    }
    public void InitPlayer()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = GameManager.gm.mainCamera;
        playerInput = GetComponent<PlayerInput>();
        currenthealth = maxHealth;
        rumble = GetComponent<Rumble>();
    }

    public void Update()
    {
        if (hasControl)
        {
            if (isShooting)
                Shoot();
            if (isPause)
                Pause();
        }
        
    }
    private void FixedUpdate()
    {
        if (hasControl)
        {
            HandleMovement();
            HandleAim();
        }
        

    }


    void HandleMovement()
    {
        if (isGameRunning)
        {
            rb.velocity = moveDirection * moveSpeed * Time.deltaTime;
        }
        
    }

    void HandleAim()
    {
        if (isGameRunning)
        {
            if (playerInput.currentControlScheme == "Gamepad")
            {


                if (stickAim == Vector3.zero)
                {
                    aimDirection = transform.position + transform.forward;
                    transform.LookAt(aimDirection);
                }

                else
                {
                    aimDirection = stickAim + transform.position;
                    transform.LookAt(aimDirection);
                }





            }
            else
            {
                aimDirection = Vector3.zero;
                Ray cameraRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                float rayLength;

                if (groundPlane.Raycast(cameraRay, out rayLength))
                {
                    aimDirection = cameraRay.GetPoint(rayLength);
                    aimDirection.y = transform.position.y;
                    transform.LookAt(aimDirection);

                }
            }

        }
    }

    public void Shoot()
    {

        AS_fire.Play();

        GameObject bulletGO = (GameObject)Instantiate(bulletPrefab, axialFirePoint.position, Quaternion.identity,GameManager.gm.bulletContainer);
        bulletGO.transform.rotation = this.gameObject.transform.rotation;
        BulletCommponent actualBullet = bulletGO.GetComponent<BulletCommponent>();
        actualBullet.currentBounces = maxBulletBounces;
        actualBullet.isShot = true;

        if (isLeftFireEnabled)
        {
            GameObject leftBulletGO = (GameObject)Instantiate(bulletPrefab, leftFirePoint.position, Quaternion.identity, GameManager.gm.bulletContainer);
            leftBulletGO.transform.rotation = leftFirePoint.rotation;
            BulletCommponent actualLeftBullet = leftBulletGO.GetComponent<BulletCommponent>();
            actualLeftBullet.currentBounces = maxBulletBounces;
            actualLeftBullet.isShot = true;
        }
        if (isRightFireEnabled)
        {
            GameObject rightBulletGO = (GameObject)Instantiate(bulletPrefab, rightFirePoint.position, Quaternion.identity, GameManager.gm.bulletContainer);
            rightBulletGO.transform.rotation = rightFirePoint.rotation;
            BulletCommponent actualRightBullet = rightBulletGO.GetComponent<BulletCommponent>();
            actualRightBullet.currentBounces = maxBulletBounces;
            actualRightBullet.isShot = true;
        }




        isShooting = false; //on termine l'input ici pour eviter de tirer en permanence tant qu'on reste appuyé
    }

    public void Pause()
    {
        Time.timeScale = 0;
        UIManager.uIm.SetFirstSelected(UIManager.Menu.Pause);
        UIManager.uIm.pausePanel.interactable = true;
        UIManager.uIm.pausePanel.alpha = 1;

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        moveDirection.x = input.x;
        moveDirection.z = input.y;
        moveDirection = moveDirection.normalized;
        

    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        isShooting = true;
        if (context.canceled)
            isShooting = false;
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        stickAim.x = input.x;
        stickAim.z = input.y;
        stickAim = stickAim.normalized;

    }

    public void OnPause(InputAction.CallbackContext context)
    {
        isPause = true;
        if (context.canceled)
            isPause = false;
    }

    public void TakeDamage()
    {
        currenthealth--;
        CameraShaker.Instance.ShakeOnce(4f, 4f, 0.1f, 0.5f);
        rumble.Play();
        GameObject particle;
        switch (currenthealth)
        {
            case 2:
                particle = (GameObject)Instantiate(GameManager.gm.explosionParticles, leftShipSection.transform.position, Quaternion.identity, GameManager.gm.particlesContainer);
                particle.GetComponent<ExplosionFXComponent>().Init(glowMaterial.GetColor("_ColorGlow"), lifetime);

                leftShipSection.SetActive(false);
                break;
            case 1:
                particle = (GameObject)Instantiate(GameManager.gm.explosionParticles, rightShipSection.transform.position, Quaternion.identity, GameManager.gm.particlesContainer);
                particle.GetComponent<ExplosionFXComponent>().Init(glowMaterial.GetColor("_ColorGlow"), lifetime);
                rightShipSection.SetActive(false);
                break;
            case 0:

                StartCoroutine(GameOver());

                break;
            default:
                break;
        }

        AS_hit.Play();
    }
     
    private void OnCollisionEnter(Collision col)
    {
      
    }
    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Enemy"))
        {
            TakeDamage();
            Destroy(col.gameObject);
        }
    }
    private void OnTriggerExit(Collider col)
    {
        
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(aimDirection, 0.1f);
    }

    public IEnumerator Init()
    {
        yield return new WaitForEndOfFrame();
        hasControl = true;
    }

    public IEnumerator GameOver()
    {
        GameManager.gm.isGameOver = true;
        GameObject particle;
        particle = (GameObject)Instantiate(GameManager.gm.explosionParticles, transform.position, Quaternion.identity, GameManager.gm.particlesContainer);
        particle.GetComponent<ExplosionFXComponent>().Init(glowMaterial.GetColor("_ColorGlow"), lifetime);
        GameManager.gm.audioSource.Play();
        AudioManager.am.Mute(AudioManager.Track.Drum2, true);
        AudioManager.am.Mute(AudioManager.Track.Guitar1, true);
        AudioManager.am.Mute(AudioManager.Track.Synth1, true);
        AudioManager.am.Mute(AudioManager.Track.Synth2, true);
        ship.SetActive(false);
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(GameManager.gm.audioSource.clip.length);
        UIManager.uIm.scoreDisplay.alpha = 0;
        UIManager.uIm.gameOver.Init();
        

    }
}
