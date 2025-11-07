using System.Collections;
using UnityEngine;

public class CommonEnemy : Enemy
{
    public Transform _left;
    public Transform _right;
    private Transform _current;
    public GameObject bulletPrefab;
    public float maxDist;
    public float _pouissance;
    private Transform firePoint;
    private float bulletSpeed= 20f;

    private void Shoot()
    {
        if (bulletPrefab && firePoint)
        {
            Vector3 direction = playerController.transform.position - transform.position;
            var look = Quaternion.LookRotation(direction);
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, look);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = bullet.transform.forward * bulletSpeed;
        }
        StartCoroutine(ShootIntervale(1));
    }


    private void Start()
    {
        _current = _left;
        firePoint = transform;
        StartCoroutine(ShootIntervale(1));
    }
    public override void DetectHealth()
    {
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
        HPText.text = hp.ToString();
    }
    public override void Move()
    {
        var rgd = GetComponent<Rigidbody>();
        if (transform.position == _left.position)
        {
            _current = _right;
        }
        else if (transform.position == _right.position)
        {
            _current = _left;
        }

        Vector3 direction = (_current.position - transform.position).normalized;
        rgd.AddForce(direction * _pouissance);
    }

    public override void Attack()
    {

    }

    IEnumerator ShootIntervale(float seconds)
    {
        // On attend le nombre de secondes demandé
        yield return new WaitForSeconds(seconds);
        Shoot();
    }
}
