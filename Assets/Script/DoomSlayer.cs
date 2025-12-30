using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class DoomSlayer : Enemy
{
    [Header("Combat")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;
    public float shootInterval = 2f;
    public int burstCount = 5;
    public float burstRate = 0.15f;

    [Header("Positionnement")]
    public Transform polygonParent; // À assigner dans l'inspecteur (parent des points du polygone)
    public float minRange = 5f;
    public float maxRange = 10f;

    private Transform player;
    private Rigidbody rb;
    private Vector3 targetPosition;
    private bool hasReachedPosition = false;

    private void Awake()
    {
        hp = 20f;
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Start()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
            player = go.transform;

        if (player)
        {
            targetPosition = GetRandomPointInPolygon();
            Debug.Log("target pos = " + targetPosition);
            StartCoroutine(MoveToPositionAndAttack());
        }
    }

    private IEnumerator MoveToPositionAndAttack()
    {
        // Déplacement vers la position cible
        while ((transform.position - targetPosition).sqrMagnitude > 0.2f)
        {
            Vector3 dir = (targetPosition - transform.position).normalized;
            rb.linearVelocity = dir * 7f;
            yield return null;
        }
        rb.linearVelocity = Vector3.zero;
        hasReachedPosition = true;
        StartCoroutine(ShootBurstRoutine());
    }

    private void FixedUpdate()
    {
        if (hasReachedPosition && player)
        {
            // Toujours regarder le joueur
            Vector3 lookDir = (player.position - transform.position).normalized;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    private IEnumerator ShootBurstRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootInterval);
            for (int i = 0; i < burstCount; i++)
            {
                Shoot();
                yield return new WaitForSeconds(burstRate);
            }
        }
    }

    private void Shoot()
    {
        if (bulletPrefab && player)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion look = Quaternion.LookRotation(direction);
            GameObject bullet = Instantiate(bulletPrefab, transform.position + direction * 1.2f, look);
            Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
            if (rbBullet != null)
                rbBullet.linearVelocity = bullet.transform.forward * bulletSpeed;
        }
    }

    public override void DetectHealth()
    {
        if (hp <= 0)
        {
            if (collectiblePrefab)
                Instantiate(collectiblePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        HPText.text = hp.ToString();
    }

    public override void Move() { }
    public override void Attack() { }

    // --- Spawn dans un polygone défini par les enfants de polygonParent ---
    private Vector3 GetRandomPointInPolygon()
    {
        // Récupère les points (vertices) du polygone
        List<Vector2> vertices = new List<Vector2>();
        foreach (Transform child in polygonParent)
        {
            vertices.Add(new Vector2(child.position.x, child.position.z));
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
}
