using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public QTEManager manager;
    [Header("Mouvement")]
    public float speed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Tir")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private Vector2 moveInput;
    private bool jumpInput;
    private bool fireInput;
    private Vector2 lookInput;
    public int bulletNumber;
    public float multiplier = 1f;
    public int multicount = 1;
    public int hp = 5;

    private bool canRotate = true;
    public bool isReloading = false;
    public BulletTrack bulletTrack;
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // --- Input Events ---
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
            jumpInput = true;
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.started)
            fireInput = true;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.started && !isReloading)
        {
            StartCoroutine(DoReloadQTE());
        }
    }

    private IEnumerator DoReloadQTE()
    {
        isReloading = true;
        canRotate = false;
        yield return StartCoroutine(QTEManager.Instance.StartQTERoutine(success =>
        {
            if (success)
            {
                ReloadWeapon();
                multiplier = 2.5f;
                multicount++;
                Debug.Log(" QTE réussi → rechargement !");
            }
            else
            {
                ReloadWeapon();
                multicount = 1;
                Debug.Log(" QTE échoué → pas de recharge.");
            }
            canRotate = true;
            isReloading = false;
        }));
    }

    private void Update()
    {
        // --- Death ---
        if (hp<=0)
        {
            Destroy(gameObject);
        }
        // --- Mouvement ---
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y); // D�placement relatif au monde
        controller.Move(move * speed * Time.deltaTime);

        if (jumpInput && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpInput = false;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- Rotation pour viser ---
        if (lookInput.sqrMagnitude > 0.1f && canRotate)
        {
            Vector3 lookDirection = new Vector3(lookInput.x, 0, lookInput.y);
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // --- Tir ---
        if (fireInput && bulletNumber>0)
        {
            Shoot();
            bulletNumber--;
            
            fireInput = false;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab && firePoint)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = firePoint.forward * bulletSpeed;
        }
    }
    

    private void ReloadWeapon()
    {
        bulletTrack.spin = 180;
        bulletTrack.Spin();
        bulletNumber = manager.nb_bullet_reload; // ou ton max
        manager.nb_bullet_reload = 0;
        Debug.Log("Arme rechargée !");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy") hp--;
    }
}

