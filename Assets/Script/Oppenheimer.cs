using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Oppenheimer : Enemy
{
    [Header("Cible")]
    private Transform player;

    [Header("Déplacement")]
    public float moveSpeed = 4f;
    public float rotationSpeed = 6f;
    public float stopDistance = 2.5f;

    [Header("Explosion")]
    public float explosionRadius = 3f;
    public int explosionDamage = 2;
    public float explosionDelay = 1f;
    public float explosionForce = 10f;

    [Header("Zone Visuelle")]
    public GameObject explosionZoneVisual;
    public float blinkSpeed = 8f;

    private Rigidbody rb;
    private bool isExploding = false;
    private Renderer zoneRenderer;

    public void SetTarget(Transform target)
    {
        player = target;
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (explosionZoneVisual)
        {
            explosionZoneVisual.SetActive(false);
            zoneRenderer = explosionZoneVisual.GetComponentInChildren<Renderer>();
        }
    }

    private void FixedUpdate()
    {
        if (!player || isExploding) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > stopDistance)
            FollowPlayer();
        else
            StopAndExplode();
    }

    private void FollowPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.linearVelocity = transform.forward * moveSpeed;
    }

    private void StopAndExplode()
    {
        rb.linearVelocity = Vector3.zero;

        if (!isExploding)
            StartCoroutine(ExplosionRoutine());
    }

    private IEnumerator ExplosionRoutine()
    {
        isExploding = true;

        if (explosionZoneVisual)
        {
            explosionZoneVisual.SetActive(true);

        }

        float timer = 0f;

        while (timer < explosionDelay)
        {
            timer += Time.deltaTime;

            if (explosionZoneVisual)
            {
                float pulse = 1f + Mathf.Sin(Time.time * blinkSpeed) * 0.15f;

                // Ne scale que X et Z, Y reste constant
                Vector3 newScale = explosionZoneVisual.transform.localScale;
                newScale.x = explosionRadius * 2f * pulse;
                newScale.z = explosionRadius * 2f * pulse;
                explosionZoneVisual.transform.localScale = newScale;
            }

            yield return null;
        }


        Explode();
    }


    private void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController pc = hit.GetComponent<PlayerController>();
                // ?? Knockback
                if (pc != null)
                {
                    pc.hp--;
                    Vector3 pushDir = (hit.transform.position - transform.position);
                    pushDir.y = 0f;

                    pc.ApplyKnockback(pushDir, explosionForce);
                }
            }
        }
        if (collectiblePrefab)
        {
            GameObject loot = Instantiate(collectiblePrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
