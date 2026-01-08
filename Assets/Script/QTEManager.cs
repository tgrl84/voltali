using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class QTEManager : MonoBehaviour
{
    bool qteResult = false;
    bool qteFinished = false;
    public enum Direction { Up, Down, Left, Right }
    [Header("UI Container")]
    public GameObject qteCanvas;

    [Header("UI Elements")]
    public Image[] arrows;                 // 4 flèches à l’écran (ou plus)
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI multText;
    public Image timerBar;                 //  Barre de progression

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip successClip;
    public AudioClip failClip;
    public AudioClip successStepClip;
    private float pitch=0.2f;

    [Header("Gameplay Settings")]
    public int easyLength = 3;
    public int normalLength = 4;
    public int hardLength = 6;
    public float timeLimit = 8f;
    public string difficulty = "normal";   // "easy", "normal", "hard"

    private PlayerControls controls;
    private List<Direction> sequence;
    private int currentIndex;
    private bool qteActive;
    private bool canRegisterInput = true;
    private float inputThreshold = 0.7f;
    private float neutralThreshold = 0.2f;
    private float timer;
    private Color multColor = new Color(247,255,0);
    public int nb_bullet_reload = 0;
    public PlayerController player;
    public static QTEManager Instance;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        controls = new PlayerControls();
    }
    /*
    void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.Reload.performed += ctx => StartQTE();
    }*/

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        if (!qteActive) return;

        timer += Time.deltaTime;
        UpdateTimerUI();

        if (timer > timeLimit)
        {
            QTEFailed();
            return;
        }

        Vector2 stick = controls.Gameplay.RightStick.ReadValue<Vector2>();
        CheckStickInput(stick);
    }

    void StartQTE()
    {
        if (qteActive) return;

        if (qteCanvas) qteCanvas.SetActive(true);

        qteActive = true;
        timer = 0f;
        currentIndex = 0;
        canRegisterInput = true;
        resultText.text = "";
        multText.text = "";
        GenerateSequence();
        UpdateUI();
        ResetTimerUI();
    }

    void GenerateSequence()
    {
        int length = difficulty switch
        {
            "easy" => easyLength,
            "hard" => hardLength,
            _ => normalLength
        };

        sequence = new List<Direction>();
        for (int i = 0; i < length; i++)
            sequence.Add((Direction)Random.Range(0, 4));
    }

    void UpdateUI()
    {
        for (int i = 0; i < arrows.Length; i++)
        {
            if (i >= sequence.Count)
            {
                arrows[i].enabled = false;
                continue;
            }

            arrows[i].enabled = true;
            Sprite s = sequence[i] switch
            {
                Direction.Up => upSprite,
                Direction.Down => downSprite,
                Direction.Left => leftSprite,
                Direction.Right => rightSprite,
                _ => null
            };
            arrows[i].sprite = s;
            arrows[i].color = Color.white;
        }
    }

    void CheckStickInput(Vector2 input)
    {
        if (input.magnitude < neutralThreshold)
        {
            canRegisterInput = true;
            return;
        }
        if (!canRegisterInput) return;

        Direction? inputDir = null;
        if (input.y > inputThreshold) inputDir = Direction.Up;
        else if (input.y < -inputThreshold) inputDir = Direction.Down;
        else if (input.x > inputThreshold) inputDir = Direction.Right;
        else if (input.x < -inputThreshold) inputDir = Direction.Left;

        if (inputDir != null)
        {
            canRegisterInput = false;

            if (inputDir == sequence[currentIndex])
            {
                // Jouer le son de réussite partielle
                if (successStepClip && audioSource)
                    audioSource.pitch = pitch;
                    audioSource.PlayOneShot(successStepClip);
                    pitch += 0.2f;

                // Feedback visuel
                if (currentIndex < arrows.Length && arrows[currentIndex] != null)
                    arrows[currentIndex].color = Color.blue;

                ///
                nb_bullet_reload = nb_bullet_reload+1;

                currentIndex++;

                if (currentIndex >= sequence.Count)
                {
                    audioSource.pitch = 1f;
                    QTESuccess();
                    pitch = 0.2f;
                }
            }
            else
            {
                arrows[currentIndex].color = Color.red;
                QTEFailed();
            }
        }
    }

    void QTESuccess()
    {
        qteActive = false;
        qteResult = true;   
        qteFinished = true;
        resultText.text = "QTE SUCCESS!";
        multColor = new Color(247 + player.multicount * 250, 255, 0);
        if (player.multicount > 1)
        {
            multText.text = $"{player.multicount} in a ROW ! Damage = {1f * player.multiplier * player.multicount}";
        }
        multText.color = multColor;
        resultText.color = Color.green;

        if (successClip && audioSource)
            audioSource.PlayOneShot(successClip);

        StartCoroutine(HideCanvasAfterDelay());
    }

    void QTEFailed()
    {
        qteActive = false;
        qteResult = false;
        qteFinished = true;
        multColor = new Color(247, 255, 0);
        resultText.text = "QTE FAILED!";
        multText.text = $"back to one...";
        multText.color = multColor;
        resultText.color = Color.red;

        if (failClip && audioSource)
            audioSource.PlayOneShot(failClip);

        StartCoroutine(HideCanvasAfterDelay());
    }

    // Gestion du timer visuel
    void UpdateTimerUI()
    {
        if (timerBar)
        {
            float fill = Mathf.Clamp01(1f - (timer / timeLimit));
            timerBar.fillAmount = fill;
        }
    }

    void ResetTimerUI()
    {
        if (timerBar)
            timerBar.fillAmount = 1f;
    }

    System.Collections.IEnumerator HideCanvasAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        if (qteCanvas) qteCanvas.SetActive(false);
    }

    public IEnumerator StartQTERoutine(System.Action<bool> onComplete)
    {
        // Reset
        qteFinished = false;
        qteResult = false;

        StartQTE();

        // Attendre qu’il se termine
        yield return new WaitUntil(() => qteFinished);

        // Callback avec le résultat (true / false)
        onComplete?.Invoke(qteResult);
        
    }
}
