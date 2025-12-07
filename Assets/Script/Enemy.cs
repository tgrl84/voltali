using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float hp = 5f;
    public PlayerController playerController;
    public Image HPbar;
    public TextMeshProUGUI HPText;

    void Update()
    {
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
        HPText.text = hp.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        // --- Balle normale ---
        if (other.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("Hit normal bullet !");
            hp -= 1f * playerController.multiplier * playerController.multicount;
            playerController.multiplier = 1f;
        }

        // --- Bombe (explosion centrale) ---
        if (other.gameObject.CompareTag("Bomb"))
        {
            Debug.Log("Hit by bomb center !");
            hp -= 10f; // dégâts de la bombe
        }
    }
}
