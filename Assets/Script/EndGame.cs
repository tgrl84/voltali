using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class EndGame : MonoBehaviour
{
    public GameObject EndScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            StartCoroutine(StopGame());
        }
    }
    // Update is called once per frame
    IEnumerator StopGame()
    {
        EndScreen.SetActive(true);
        yield return new WaitForSeconds(1.2f);
        SceneManager.LoadScene("Menu");
        yield return new WaitForSeconds(1.2f);
    }
}
