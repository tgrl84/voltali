using UnityEngine;

public class Walls : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Enemy"))
        {
            Physics.IgnoreCollision(col.collider, GetComponent<Collider>());
        }
    }
}
