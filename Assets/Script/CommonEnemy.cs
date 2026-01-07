using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonEnemy : Enemy
{
    public GameObject bulletPrefab;
    public float maxDist;
    public float moveSpeed = 5f; // Vitesse de déplacement
    private Transform firePoint;
    private float bulletSpeed = 20f;
    private Rigidbody rb;

    [Header("Zone de déplacement")]
    public Transform polygonParent; // Parent des points du polygone (zone de jeu)
    private Vector3 targetPosition;
    private float reachThreshold = 1.5f; // Distance pour considérer le point atteint
    private float slowDownDistance = 3f; // Distance à partir de laquelle on ralentit

    private void Shoot()
    {
        if (bulletPrefab && firePoint)
        {
            Vector3 direction = playerController.transform.position - transform.position;
            var look = Quaternion.LookRotation(direction);
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, look);
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
                bulletRb.linearVelocity = bullet.transform.forward * bulletSpeed;
        }
        StartCoroutine(ShootIntervale(1));
    }

    private void Awake()
    {
        // Récupérer automatiquement la zone de jeu via le tag "GameZone"
        GameObject zone = GameObject.FindGameObjectWithTag("GameZone");
        if (zone != null)
            polygonParent = zone.transform;
        
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        
        scoreValue = 30; // Score pour CommonEnemy
    }

    private void Start()
    {
        firePoint = transform;
        // Initialiser la première cible de déplacement
        if (polygonParent != null)
        {
            targetPosition = GetRandomPointInPolygon();
            Debug.Log($"CommonEnemy target position: {targetPosition}");
        }
        else
            targetPosition = transform.position;
        StartCoroutine(ShootIntervale(1));
    }

    public override void DetectHealth()
    {
        if (hp <= 0)
        {
            Die();
        }
        HPText.text = hp.ToString();
    }

    protected override void Die()
    {
        // Ajouter le score
        if (gameManager != null)
            gameManager.AddScore(scoreValue);
            
        if (collectiblePrefab)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            Instantiate(collectiblePrefab, spawnPos, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    public override void Move()
    {
        if (rb == null) return;

        // Calcul de la distance à la cible (en ignorant Y)
        Vector3 flatPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatTarget = new Vector3(targetPosition.x, 0, targetPosition.z);
        float distanceToTarget = Vector3.Distance(flatPosition, flatTarget);
        
        // Si on a atteint la cible, en choisir une nouvelle
        if (distanceToTarget < reachThreshold)
        {
            // Arrêter le mouvement
            rb.linearVelocity = Vector3.zero;
            
            // Choisir une nouvelle position aléatoire dans la zone
            if (polygonParent != null)
            {
                targetPosition = GetRandomPointInPolygon();
                Debug.Log($"CommonEnemy new target: {targetPosition}");
            }
            return;
        }

        // Direction vers la cible
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;

        // Calcul de la vitesse avec ralentissement progressif à l'approche
        float speedMultiplier = 1f;
        if (distanceToTarget < slowDownDistance)
        {
            speedMultiplier = distanceToTarget / slowDownDistance;
            speedMultiplier = Mathf.Clamp(speedMultiplier, 0.2f, 1f); // Vitesse minimum 20%
        }

        // Appliquer la vélocité directement (pas AddForce qui accumule)
        rb.linearVelocity = direction * moveSpeed * speedMultiplier;
    }

    public override void Attack()
    {
    }

    IEnumerator ShootIntervale(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Shoot();
    }

    // --- Génération d'un point aléatoire dans un polygone défini par les enfants de polygonParent ---
    private Vector3 GetRandomPointInPolygon()
    {
        if (polygonParent == null)
            return transform.position;

        // Récupère les points (vertices) du polygone en utilisant les positions locales
        List<Vector2> vertices = new List<Vector2>();
        foreach (Transform child in polygonParent)
        {
            Vector3 worldPos = polygonParent.TransformPoint(child.localPosition);
            vertices.Add(new Vector2(worldPos.x, worldPos.z));
        }

        if (vertices.Count < 3)
        {
            Debug.LogWarning("Il faut au moins 3 points pour définir une zone !");
            return transform.position;
        }

        // Triangulation du polygone (convexe uniquement)
        List<(Vector2, Vector2, Vector2)> triangles = new List<(Vector2, Vector2, Vector2)>();
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles.Add((vertices[0], vertices[i], vertices[i + 1]));
        }

        // Choisit un triangle au hasard
        var tri = triangles[Random.Range(0, triangles.Count)];

        // Génère un point aléatoire dans le triangle choisi
        Vector2 p = RandomPointInTriangle(tri.Item1, tri.Item2, tri.Item3);

        // Place le point au bon Y
        float y = transform.position.y;

        return new Vector3(p.x, y, p.y);
    }

    // Génère un point aléatoire dans un triangle (barycentrique)
    private Vector2 RandomPointInTriangle(Vector2 a, Vector2 b, Vector2 c)
    {
        float r1 = Random.value;
        float r2 = Random.value;
        if (r1 + r2 > 1f)
        {
            r1 = 1f - r1;
            r2 = 1f - r2;
        }
        Vector2 point = a + r1 * (b - a) + r2 * (c - a);
        return point;
    }

    private void OnDrawGizmosSelected()
    {
        // Dessiner la position cible pour debug
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
        Gizmos.DrawLine(transform.position, targetPosition);
        
        // Dessiner la zone de ralentissement
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPosition, slowDownDistance);
    }
}
