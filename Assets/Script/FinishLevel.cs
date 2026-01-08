using UnityEngine;

public class FinishLevel : MonoBehaviour
{
    public GameObject Arrow;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Stop()
    {
        Arrow.SetActive(true);
        gameObject.SetActive(false);
    }
}
