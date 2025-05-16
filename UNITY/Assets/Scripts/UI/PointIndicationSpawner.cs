using UnityEngine;

public class PointIndicationSpawner : MonoBehaviour
{

    [SerializeField] private GameObject spawnedObject;

    public static PointIndicationSpawner Instance;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    public void Spawn(int points, Color color, Vector3 worldLocation)
    {
        GameObject newObj = Instantiate(spawnedObject, transform);
        PointIndicator pi = newObj.GetComponent<PointIndicator>();
        pi.SetVisuals($"+{points}", color);
        newObj.transform.localScale = Vector3.one * Mathf.Clamp((points+100)/250, 0.5f, 2f);
        newObj.transform.position = Camera.main.WorldToScreenPoint(worldLocation) + Vector3.up * 20f;
        pi.PlayAnimation();
    }
}
