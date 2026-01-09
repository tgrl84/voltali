using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Pause : MonoBehaviour
{
    public GameObject MenuPause;
    private InputAction pause;
    private bool isPaused;
    public Image[] Buttons;
    private float lastInputTime;
    public Vector2 moveInput;
    public bool submitPressed;
    public int selectedIndex = 0;
    private float inputCooldown = 0.2f;


    void Update()
    {
        if (!isPaused)
            return;

        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            moveInput = gamepad.leftStick.ReadValue();
            submitPressed = gamepad.buttonSouth.wasPressedThisFrame; 
        }

        // Navigation gauche/droite avec cooldown
        if (Time.unscaledTime - lastInputTime > inputCooldown)
        {
            if (moveInput.y > 0.5f)
            {
                selectedIndex = (selectedIndex + 1) % Buttons.Length;
                lastInputTime = Time.unscaledTime;
            }
            else if (moveInput.y < -0.5f)
            {
                selectedIndex = (selectedIndex - 1 + Buttons.Length) % Buttons.Length;
                lastInputTime = Time.unscaledTime;
            }
        }

        // Surbrillance de l’icône sélectionnée
        for (int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].color = (i == selectedIndex) ? Color.yellow : Color.white;
        }

        if (submitPressed)
        {
            Navigation(selectedIndex); 
        }
    }

    void Navigation(int index)
    {
        if (!isPaused)
            return;
        switch (index)
        {
            case 0: TogglePause(); break;
            case 1: Quit(); break;
        }
        submitPressed = false;
        MenuPause.SetActive(false);
        Time.timeScale = 1;
    }

    public void Quit()
    {
        SceneManager.LoadScene("Menu");
    }

    void Awake()
    {
        pause = new InputAction("Pause", InputActionType.Button, "<Keyboard>/Tab");
        pause = new InputAction("Pause", InputActionType.Button, "<Gamepad>/Start");
        pause.performed += _ => TogglePause();
        pause.Enable();
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        MenuPause.SetActive(isPaused);
        Debug.Log("Pause : " + isPaused);
    }

    void Start()
    {
        submitPressed = false;
        MenuPause.SetActive(false);
    }
}