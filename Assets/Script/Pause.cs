using UnityEngine;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    public GameObject MenuPause;
    private InputAction pause;
    private bool isPaused;

    void Awake()
    {
        pause = new InputAction("Pause", InputActionType.Button, "<Keyboard>/Tab");
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
        MenuPause.SetActive(false);
    }
}