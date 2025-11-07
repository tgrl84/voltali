using UnityEngine;
using UnityEngine.UI;
public class BulletTrack : MonoBehaviour
{
    public PlayerController controller;
    public Sprite[] sprites;
    public Image self;
    public float spin = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        self.sprite = sprites[6];
    }

    // Update is called once per frame
    void Update()
    {
        self.sprite = sprites[controller.bulletNumber];
        spin= controller.bulletNumber*60;
        Quaternion target = Quaternion.Euler(0, 0, spin);

        // Dampen towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 5f);
    }

    public void Spin()
    {
        Quaternion target = Quaternion.Euler(0, 0, spin);

        // Dampen towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 5f);
    }
}
