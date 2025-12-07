using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPPannel : MonoBehaviour
{
    public Sprite[] sprites;
    public PlayerController player;
    public Image self;

    void Start()
    {
        self.sprite = sprites[5];
    }

    void Update()
    {
        if (player)
        {
            // transforme 0-20 HP → index 0-5
            int index = Mathf.Clamp(Mathf.FloorToInt(player.hp / 4f), 0, 5);

            self.sprite = sprites[index];
        }
    }
}