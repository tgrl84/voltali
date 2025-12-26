using UnityEngine;

public class Collectibles : MonoBehaviour
{
    public int nbSteel = 1; 

    private void OnTriggerEnter(Collider other)
    {
        // Vérifie si c'est le joueur
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && other.tag=="Player")
        {
            // Ajoute du steel au joueur
            player.nbSteel++;

            // Détruit le collectible après collecte
            Destroy(gameObject);
        }
    }
}