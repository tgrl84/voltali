using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

[RequireComponent(typeof(Rigidbody))]
public class Oppenheimer : Enemy
{
    private Transform player;

    [Header("Déplacement")]
    public float moveSpeed = 4f;
    public float rotationSpeed = 6f;
    public float explosionTriggerDistance = 2.5f;

    [Header("Explosion")]
    public float explosionRadius = 3f;
    public int explosionDamage = 2;
    public float explosionDelay = 3f;
    public float explosionForce = 10f;
    public GameObject explosionPrefab;
    public GameObject explosionSound;

    [Header("Zone Visuelle")]
    public GameObject explosionZoneVisual;
    public float blinkSpeed = 8f;

    private Rigidbody rb;
    private bool isExploding = false;
    private int scoreValue;
    private GameManager gameManager;

    private void Awake()
    {
        hp = 8f;
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        if (explosionZoneVisual)
            explosionZoneVisual.SetActive(false);
        scoreValue = 50; // Score pour Oppenheimer
    }

    public void SetTarget(Transform target)
    {
        player = target;
    }

    private void FixedUpdate()
    {
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
                player = go.transform;
            if (!player) return;
        }
        if (isExploding) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > explosionTriggerDistance)
        {
            MoveTowardsPlayer();
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
            StartCoroutine(ExplosionRoutine());
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        rb.linearVelocity = transform.forward * moveSpeed;
    }

    private IEnumerator ExplosionRoutine()
    {
        isExploding = true;

        if (explosionZoneVisual)
        {
            explosionZoneVisual.SetActive(true);
            explosionZoneVisual.transform.localScale = Vector3.one * explosionRadius * 4f;
        }

        float timer = 0f;
        while (timer < explosionDelay)
        {
            timer += Time.deltaTime;
            if (explosionZoneVisual)
            {
                float pulse = 1f + Mathf.Sin(Time.time * blinkSpeed) * 0.15f;
                var scale = Vector3.one * explosionRadius * 4f * pulse;
                scale.y = 0.1f;
                explosionZoneVisual.transform.localScale = scale;
            }
            yield return null;
        }

        Explode();
    }

    private void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        HashSet<PlayerController> alreadyHit = new HashSet<PlayerController>();

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var pc = hit.GetComponent<PlayerController>();
                if (pc != null && !alreadyHit.Contains(pc))
                {
                    pc.OnTakeDamage(explosionDamage);
                    Vector3 pushDir = hit.transform.position - transform.position;
                    pushDir.y = 0f;
                    pc.ApplyKnockback(pushDir, explosionForce);
                    alreadyHit.Add(pc);
                }
            }
        }

        // Instanciation du prefab d'explosion
        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // Lecture du son d'explosion
        if (explosionSound)
            Instantiate(explosionSound, transform.position, Quaternion.identity);

        if (explosionZoneVisual)
            explosionZoneVisual.SetActive(false);

        Die();
    }

    protected override void Die()
    {
        // Ajouter le score
        if (gameManager != null)
            gameManager.AddScore(scoreValue);
            
        if (collectiblePrefab)
            Instantiate(collectiblePrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
