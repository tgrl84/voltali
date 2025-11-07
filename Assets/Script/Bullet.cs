using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f; 
    public float damage = 1f;
    [SerializeField] string tagDestroy = "Player";

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == tagDestroy) return;
        Destroy(gameObject);
    }

}