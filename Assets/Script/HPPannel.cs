using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPPannel : MonoBehaviour
{
    public Sprite[] sprites;
    public PlayerController player; // référence au joueur
    public Image self;
    void Start()
    {
        self.sprite = sprites[5];
    }
    void Update()
    {
        if (player)
        {
            self.sprite = sprites[player.hp];
        }
    }
}