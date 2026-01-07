using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SHAEnnemy : Enemy
{
    [Header("Cible")]
    private Transform player;

    [Header("SHA Settings")]
    public float chargeSpeed = 8f;
    public float bounceForce = 12f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float rotationSpeed = 10f;
    public float bounceDistance = 3f;

    [Header("Rebond & Recul")]
    public float bounceDuration = 0.3f;
    public float turnAroundTime = 0.5f;
    public float retreatDistance = 6f;
    public float retreatSpeed = 5f;

    [Header("Anti-blocage")]
    public float stuckCheckTime = 2f;
    public float minimumMovementThreshold = 0.1f;

    [Header("Effets d'explosion")]
    public GameObject explosionPrefab;
    public GameObject explosionSoundPrefab;

    // États internes
    private bool isCharging, isBouncing, isTurningAround, isRetreating, canAttack = true;
    private Rigidbody rb;
    private Vector3 bounceDirection, retreatTarget;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;

    public void SetTarget(Transform target) => player = target;

    private void Awake()
    {
        hp = 4f;
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        lastPosition = transform.position;
        scoreValue = 15; // Score pour SHA
    }

    private void FixedUpdate()
    {
        if (!player) return;
        CheckIfStuck();

        if (isBouncing)
            HandleBounce();
        else if (isTurningAround)
            return; // Demi-tour géré par coroutine
        else if (isRetreating)
            HandleRetreat();
        else if (canAttack)
            ChargeTowardsPlayer();
    }

    private void CheckIfStuck()
    {
        float movement = Vector3.Distance(transform.position, lastPosition);
        if (movement < minimumMovementThreshold && (isCharging || isRetreating))
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckCheckTime)
                HandleStuckSituation();
        }
        else
        {
            stuckTimer = 0f;
        }
        lastPosition = transform.position;
    }

    private void HandleStuckSituation()
    {
        rb.linearVelocity = Vector3.zero;
        isCharging = isBouncing = isRetreating = isTurningAround = false;
        StopAllCoroutines();
        stuckTimer = 0f;
        if (!canAttack)
            StartCoroutine(RestartAfterStuck());
    }

    private IEnumerator RestartAfterStuck()
    {
        yield return new WaitForSeconds(0.5f);
        canAttack = true;
    }

    private void ChargeTowardsPlayer()
    {
        isCharging = true;
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        rb.linearVelocity = transform.forward * chargeSpeed;
    }

    private void HandleRetreat()
    {
        Vector3 retreatDir = (retreatTarget - transform.position).normalized;
        retreatDir.y = 0f;
        rb.linearVelocity = retreatDir * retreatSpeed;
        if (Vector3.Distance(transform.position, retreatTarget) < 0.5f)
        {
            isRetreating = false;
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void OnCollisionWithPlayer()
    {
        if (!canAttack || !isCharging) return;

        if (playerController != null)
        {
            playerController.OnTakeDamage(1);
            if (explosionPrefab)
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            if (explosionSoundPrefab)
                Instantiate(explosionSoundPrefab, transform.position, Quaternion.identity);
            takeDamage(2);
            
        }
        StartBounce();
        StartCoroutine(AttackCooldown());
    }

    private void StartBounce()
    {
        isCharging = false;
        isBouncing = true;
        bounceDirection = (transform.position - player.position).normalized;
        bounceDirection.y = 0f;
        rb.linearVelocity = bounceDirection * bounceForce;
        StartCoroutine(BounceSequence());
    }

    private IEnumerator BounceSequence()
    {
        yield return new WaitForSeconds(bounceDuration);
        rb.linearVelocity = Vector3.zero;
        isBouncing = false;
        isTurningAround = true;
        yield return StartCoroutine(TurnAround180());
        StartRetreat();
        isTurningAround = false;
    }

    private void StartRetreat()
    {
        isRetreating = true;
        Vector3 dirFromPlayer = (transform.position - player.position).normalized;
        dirFromPlayer.y = 0f;
        retreatTarget = transform.position + (dirFromPlayer * retreatDistance);
    }

    private IEnumerator TurnAround180()
    {
        Vector3 startRot = transform.eulerAngles;
        Vector3 targetRot = startRot + new Vector3(0, 180, 0);
        float elapsed = 0f;
        while (elapsed < turnAroundTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / turnAroundTime;
            transform.eulerAngles = Vector3.Lerp(startRot, targetRot, t);
            yield return null;
        }
        transform.eulerAngles = targetRot;
    }

    private void HandleBounce()
    {
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, 2f * Time.fixedDeltaTime);
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player") && (isCharging || isRetreating))
        {
            stuckTimer = stuckCheckTime;
        }
    }

    // Overrides inutiles mais nécessaires pour éviter les conflits
    public override void Move() { }
    public override void Attack() { }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Bullet")
        {
            takeDamage((playerController.dmg * playerController.multiplier * playerController.multicount) * 2f);
            playerController.multiplier = 1f;
        }
        if (other.gameObject.tag == "Player" && isCharging && canAttack)
        {
            OnCollisionWithPlayer();
        }
    }

    protected override void Die()
    {
        // Ajouter le score
        if (gameManager != null)
            gameManager.AddScore(scoreValue);
            
        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        if (explosionSoundPrefab)
            Instantiate(explosionSoundPrefab, transform.position, Quaternion.identity);
        if (collectiblePrefab)
            Instantiate(collectiblePrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, bounceDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
}