using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int score;
    public Vector2 moveInput;
    public bool submitPressed;
    public GameObject BonusHUD;
    public PlayerController pc;
    public int EnemyNumber;
    public FinishLevel FinishLevel;
    public TextMeshProUGUI ScoreText;
    public Image ScoreBlank;
    public Image ScoreImage_Add;
    public Image ScoreNumber;
    public Image ScoreNumberAddBlank;
    public Sprite[] ImageList;
    public Sprite[] ImageAdd;
    private List<int> ScoreBonus = new List<int> { 50, 100, 150, 200, 250, 300, 350, 400, 450, 500 };
    public int selectedIndex = 0; // 0 = dmg, 1 = speed, 2 = bullet speed
    public Image[] BonusIcons; // assigner les icônes des 3 bonus dans l'inspecteur
    private float inputCooldown = 0.2f; // éviter que l’axe soit lu plusieurs fois trop vite
    private float lastInputTime;
    void Update()
    {
        // Vérification du score pour le pop-up de bonus
        if (score >= ScoreBonus[0])
        {
            BonusPopUp();
            ScoreBonus.RemoveAt(0);
        }

        // Vérification de la fin de niveau
        if (EnemyNumber == 0)
        {
            FinishLevel.Stop();
        }

        // Mise à jour du texte du score
        ScoreText.text = score.ToString();

        // Gestion du HUD de bonus
        if (BonusHUD.activeSelf)
        {
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                moveInput = gamepad.leftStick.ReadValue(); // stick gauche ou D-pad
                submitPressed = gamepad.buttonSouth.wasPressedThisFrame; // bouton A / Cross
            }

            // Navigation gauche/droite avec cooldown
            if (Time.unscaledTime - lastInputTime > inputCooldown)
            {
                if (moveInput.x > 0.5f)
                {
                    selectedIndex = (selectedIndex + 1) % BonusIcons.Length;
                    lastInputTime = Time.unscaledTime;
                }
                else if (moveInput.x < -0.5f)
                {
                    selectedIndex = (selectedIndex - 1 + BonusIcons.Length) % BonusIcons.Length;
                    lastInputTime = Time.unscaledTime;
                }
            }

            // Surbrillance de l’icône sélectionnée
            for (int i = 0; i < BonusIcons.Length; i++)
            {
                BonusIcons[i].color = (i == selectedIndex) ? Color.yellow : Color.white;
            }

            // Validation du bonus
            if (submitPressed)
            {
                ApplyBonus(selectedIndex);
            }
        }
    }

    void ApplyBonus(int index)
    {
        switch (index)
        {
            case 0: addDmg(); break;
            case 1: addSpeed(); break;
            case 2: addbulletSpeed(); break;
        }
        BonusHUD.SetActive(false);
        Time.timeScale = 1; 
    }

    void Start()
    {
        score = 0;
        ScoreNumber.sprite=ImageList[0];
        ScoreImage_Add.gameObject.SetActive(false);
        StartCoroutine(EventLoop());
    }



    public void AddScore(int adds)
    {
        StartCoroutine(AnimAddScore(adds));
    }

    public void BonusPopUp()
    {

        BonusHUD.SetActive(true);
        Time.timeScale = 0;
    }

    public void addDmg()
    {
        pc.dmg *= 1.5f;
        BonusHUD.SetActive(false);
        Time.timeScale = 1;
    }
    public void addSpeed()
    {
        pc.speed += 2f;
        BonusHUD.SetActive(false);
        Time.timeScale = 1;
    }
    public void addbulletSpeed()
    {
        pc.bulletSpeed += 10f;
        BonusHUD.SetActive(false);
        Time.timeScale = 1;
    }
    IEnumerator EventLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            AddScore(10);
        }
    }
    IEnumerator AnimAddScore(int adds)
    {
        
        ScoreImage_Add.gameObject.SetActive(true);
        ScoreNumberAddBlank.sprite = ImageAdd[adds / 10];
        
        yield return new WaitForSeconds(1.5f);
        score += adds;
        ScoreNumber.sprite = ImageList[score/10];
        ScoreImage_Add.gameObject.SetActive(false);

        yield return new WaitForSeconds(1.2f);
    }
}
