using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int score;
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
    void Start()
    {
        score = 0;
        ScoreNumber.sprite=ImageList[0];
        ScoreImage_Add.gameObject.SetActive(false);
        StartCoroutine(EventLoop());
    }

    // Update is called once per frame
    void Update()
    {

        if (EnemyNumber == 0) {
            FinishLevel.Stop();
        }
        ScoreText.text = score.ToString();
        
    }


    public void AddScore(int adds)
    {
        StartCoroutine(AnimAddScore(adds));
        if (score >= ScoreBonus[0])
        {
            BonusPopUp();
            ScoreBonus.RemoveAt(0);
        }
    }

    void BonusPopUp()
    {

    }

    IEnumerator EventLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
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
