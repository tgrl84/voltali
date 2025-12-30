using UnityEngine;

public class auto_destroy : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f; // Lifetime in seconds
    private float timer;
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
