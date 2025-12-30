using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
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

    public void updatePlayerHealthUI(int hp)
    {
        self.sprite = sprites[hp];
    }
}