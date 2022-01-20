using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


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
    public GameObject firePoint;
    private Rigidbody rb;
    private Camera mainCamera;
    public GameObject bulletContainer;
    PlayerInput playerInput;

    [Space]
    [Header("Controls")]
    [Space]

    public float moveSpeed;
    private Vector3 moveDirection;
    Vector3 aimDirection;
    Vector3 stickAim;

    [Space]
    [Header("Controls")]
    [Space]
    public bool isShooting;
    public bool isPause;
    
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = FindObjectOfType<Camera>();
        playerInput = GetComponent<PlayerInput>();
        currenthealth = maxHealth;
        
    }

    public void Update()
    {
        
        if (isShooting)
            Shoot();
        if (isPause)
            Pause();
    }
    private void FixedUpdate()
    {
        HandleMovement();
        HandleAim();

    }


    void HandleMovement()
    {
        rb.velocity = moveDirection * moveSpeed * Time.deltaTime;
    }

    void HandleAim()
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

    public void Shoot()
    {
        GameObject bulletGO = (GameObject)Instantiate(bulletPrefab, firePoint.transform.position, Quaternion.identity,bulletContainer.transform);
        bulletGO.transform.rotation = this.gameObject.transform.rotation;
        BulletCommponent actualBullet = bulletGO.GetComponent<BulletCommponent>();
        actualBullet.currentBounces = maxBulletBounces;
        actualBullet.isShot = true;


        isShooting = false; //on termine l'input ici pour eviter de tirer en permanence tant qu'on reste appuy�
    }

    public void Pause()
    {
        Time.timeScale = 0;
        UIManager.uIm.pauseMenu.SetActive(true);
        CanvasGroup canvas = UIManager.uIm.mainCanvas.GetComponent<CanvasGroup>();
        canvas.interactable = true;
        canvas.alpha = 1;
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
        switch (currenthealth)
        {
            case 2:
                //Anim destruction/particle system
                Destroy(leftShipSection);
                break;
            case 1:
                //Anim destruction/particle system
                Destroy(rightShipSection);
                break;
            default:
                //Lose Condition
                break;
        }
    }
    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Spawner"))
        {
            col.gameObject.GetComponent<SpawnerComponent>().isPlayerNear(true);
        }
        if (col.CompareTag("Enemy"))
        {
            TakeDamage();
            Destroy(col.gameObject);
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Spawner"))
        {
            col.gameObject.GetComponent<SpawnerComponent>().isPlayerNear(false);
        }
    }
   

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(aimDirection, 0.1f);
    }
}