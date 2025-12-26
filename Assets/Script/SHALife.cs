using UnityEngine;
using System;

public class SHALife : MonoBehaviour
{
    public Action OnDeath;

    private void OnDestroy()
    {
        OnDeath?.Invoke();
    }
}