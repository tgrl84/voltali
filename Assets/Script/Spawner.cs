using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [Header("Spawn")]
    public Transform player;
    public GameObject SHAPrefab;
    public Transform spawnPoint;

    [Header("Timing")]
    public float spawnInterval = 20f;

    private int currentDrones = 0;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
                SpawnDrone();
            

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnDrone()
    {
        GameObject drone = Instantiate(SHAPrefab, spawnPoint.position, spawnPoint.rotation);
        currentDrones++;

        KamikazeDroneEnemy enemy = drone.GetComponent<KamikazeDroneEnemy>();
        if (enemy != null)
        {
            enemy.SetTarget(player);
        }

        SHALife life = drone.GetComponent<SHALife>();
        if (life != null)
        {
            life.OnDeath += OnDroneDeath;
        }
    }

    private void OnDroneDeath()
    {
        currentDrones--;
    }
}
