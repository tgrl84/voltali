using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public QTEManager manager;
    public GameManager gameManager;
    [Header("Mouvement")]
    public float speed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    [Header("Knockback")]
    public float knockbackForce = 8f;
    private Vector3 knockbackVelocity;
    [Header("Tir")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    [Header("Cooldown Tir")]
    public float fireCooldown = 0.4f; // délai entre chaque balle
    private bool canShoot = true;

    public int nbSteel = 3;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip gunClip;

    private Vector2 moveInput;
    private bool jumpInput;
    private bool fireInput;
    private Vector2 lookInput;
    public int bulletNumber;
    public float multiplier = 1f;
    public int multicount = 1;
    public int hp = 5;
    public float dmg = 1f;
    public bool TriShotGun=false;
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
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.tag == "BulletEnemy")
        {
            OnTakeDamage(1);
        }
    }

    public void OnTakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
        new WaitForSeconds(2f);
        Time.timeScale = 0;
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
        if (context.started && !isReloading && nbSteel>0)
        {
            StartCoroutine(DoReloadQTE());
        }
    }
    void ReloadWeapon()
    {
        bulletTrack.spin = 180;
        bulletTrack.Spin();
        bulletNumber = manager.nb_bullet_reload; // ou ton max
        manager.nb_bullet_reload = 0;
        nbSteel--;
        Debug.Log("Arme rechargée !");
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

        if (knockbackVelocity.magnitude > 0.1f)
        {
            controller.Move(knockbackVelocity * Time.deltaTime);
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, 8f * Time.deltaTime);
        }

        // --- Rotation pour viser ---
        if (lookInput.sqrMagnitude > 0.1f && canRotate)
        {
            Vector3 lookDirection = new Vector3(lookInput.x, 0, lookInput.y);
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // --- Tir ---
        if (fireInput && bulletNumber > 0 && canShoot && !isReloading)
        {
            StartCoroutine(FireRoutine());
            fireInput = false;
        }
    }
    private IEnumerator FireRoutine()
    {
        canShoot = false;
        if (gunClip && audioSource) { 
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.PlayOneShot(gunClip);
            audioSource.pitch = 1f;
        }
        if (TriShotGun)
        {
            yield return StartCoroutine(TriShotRoutine());
        }
        else
        {
            Shoot();
        }

        bulletNumber--;

        yield return new WaitForSeconds(fireCooldown);
        canShoot = true;
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
    public float spreadAngle = 15f;

    private IEnumerator TriShotRoutine()
    {
        ShootBullet(0f);
        yield return new WaitForSeconds(0.1f);

        ShootBullet(-spreadAngle);
        yield return new WaitForSeconds(0.1f);

        ShootBullet(+spreadAngle);
    }

    private void ShootBullet(float angleOffset)
    {
        Quaternion rot = firePoint.rotation * Quaternion.Euler(0f, 0f, angleOffset);

        // Spawn légèrement en avant pour éviter que la balle touche le joueur
        Vector3 spawnPos = firePoint.position + firePoint.forward * 0.3f;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, rot);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = bullet.transform.forward * bulletSpeed;
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        knockbackVelocity = direction.normalized * force;
    }
}


    


