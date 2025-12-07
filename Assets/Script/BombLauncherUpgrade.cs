using UnityEngine;

public class BombLauncherUpgrade : MonoBehaviour
{
    public GameObject bombPrefab;
    public Transform firePoint;
    public int magazine = 2;
    public float force = 15f;

    private int ammo;

    void Start()
    {
        ammo = magazine;
    }

    public void ShootBomb()
    {
        if (ammo <= 0) return;

        GameObject bomb = Instantiate(bombPrefab, firePoint.position, firePoint.rotation);
        bomb.tag = "Bomb";

        Rigidbody rb = bomb.GetComponent<Rigidbody>();
        rb.linearVelocity = firePoint.forward * force;

        ammo--;
    }

    public void Reload()
    {
        ammo = magazine;
    }
}