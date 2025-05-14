using UnityEngine;
using UnityEngine.UI;

public class DebugCamera : MonoBehaviour
{

    private Image img;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        img = GetComponent<Image>();
        Client.network_instance.startCameraFeed(img);
    }
}
