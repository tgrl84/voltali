using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float hp = 5f;
    public PlayerController playerController;
    public Image HPbar;
    public GameObject collectiblePrefab;
    public TextMeshProUGUI HPText;
    // Start is called once before the first execution of Update after the MonoBehaviour is create
    void Start()
    {
        
    }

    public virtual void DetectHealth()
    {
        HPText.text = hp.ToString();
    }
    public virtual void Move()
    {


    }
    public virtual void Attack()
    {

    }
    // Update is called once per frame
    void Update()
    {
        DetectHealth();
        Move();
        Attack();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Bullet")
        {
            Debug.Log("hit !");
            hp -= playerController.dmg * playerController.multiplier * playerController.multicount;
            playerController.multiplier = 1f;

            if (hp <= 0)
                Die();
        }
    }

    private void Die()
    {
        if (collectiblePrefab)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            Instantiate(collectiblePrefab, spawnPos, Quaternion.identity);
        }
        Destroy(gameObject);
    }

}
