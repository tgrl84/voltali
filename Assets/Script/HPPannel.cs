using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class HPPannel : MonoBehaviour
{
    public Sprite[] sprites;
    public PlayerController player; // référence au joueur
    public Image self;
    public Image[] Steels;
    void Start()
    {
        self.sprite = sprites[5];
    }

    public void updatePlayerHealthUI(int hp)
    {
        self.sprite = sprites[hp];
    }

    public void updatePlayerSteelsUI(int nbSteel)
    {
        for (int i = 2; i >= nbSteel-1; i--)
        {
            Steels[i].gameObject.SetActive(false);
        }
    }

    public void updatePlayerSteelsAddUI(int nbSteel)
    {
        for (int i = 0; i <= nbSteel - 1; i++)
        {
            Steels[i].gameObject.SetActive(true);
        }
    }
}