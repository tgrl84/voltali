using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float hp = 5f;
    public PlayerController playerController;
    public Image HPbar;
    public TextMeshProUGUI HPText;
    // Start is called once before the first execution of Update after the MonoBehaviour is create
    void Start()
    {
        
    }

    public virtual void DetectHealth()
    {
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
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
            hp = hp - (1f * playerController.multiplier * playerController.multicount);
            playerController.multiplier = 1f;
        }
    }
    
}
