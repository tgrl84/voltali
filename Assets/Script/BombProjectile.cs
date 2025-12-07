using UnityEngine;

public class BombProjectile : MonoBehaviour
{
    public float aoeDamage = 10f;
    public float radius = 3f;

    void OnCollisionEnter(Collision collision)
    {
     
        // Détection AOE
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hits)
        {
            // Ennemi touché
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null)
            {
                e.hp -= aoeDamage;
            }

            // Joueur touché
            PlayerController p = hit.GetComponent<PlayerController>();
            if (p != null)
            {
                p.hp -= aoeDamage; 
            }
        }

        Destroy(gameObject);
    }

    void Start()
    {
        
        Destroy(gameObject, 5f);
    }
}