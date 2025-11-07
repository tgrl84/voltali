using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public Camera cam;       // Ta caméra principale
    public RectTransform uiElement;

    public Vector3 offset;   // Décalage en pixels ou unité UI

    void Update()
    {
        if (player != null && cam != null)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(player.position + offset);
            uiElement.position = screenPos;
        }
    }
}