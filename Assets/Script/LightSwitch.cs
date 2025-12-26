using UnityEngine;
using System.Collections;


public class LightSwitch : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject light1;
    public GameObject light2;
    void Start()
    {
        light1.SetActive(true);
        light2.SetActive(false);
        StartCoroutine(GyroLight());
    }

    IEnumerator GyroLight()
    {
        while (true)
        {
            // Inverse les lumières
            light1.SetActive(!light1.activeSelf);
            light2.SetActive(!light2.activeSelf);

            // Attend 0.2 secondes avant le prochain switch
            yield return new WaitForSeconds(0.4f);
        }
    }
}
