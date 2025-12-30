using UnityEngine;

public class HudController : MonoBehaviour
{
    // Singleton
    public static HudController Instance { get; private set; }

    // Référence à HPPannel à assigner dans l'Inspector
    [SerializeField] private HPPannel hpPannel;
    public HPPannel HPPannel => hpPannel;

    private void Awake()
    {
        // Implémentation du singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
