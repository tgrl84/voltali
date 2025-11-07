using TMPro;
using UnityEngine;

public class AmmoPannel : MonoBehaviour
{
    public TextMeshProUGUI ammoText;
    public PlayerController player; // référence au joueur

    void Update()
    {
        if (player)
        {
            ammoText.text = $"{player.bulletNumber} / 6";
        }
    }
}