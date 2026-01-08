using UnityEngine;
using static PlayerController;

public class LaserDamage : MonoBehaviour
{
    [SerializeField] float damagePerSecond = 1f;

    private void OnTriggerStay(Collider other)
    {
        IDamageable dmg = other.GetComponent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}

