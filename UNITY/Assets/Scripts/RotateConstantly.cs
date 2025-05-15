using UnityEngine;

public class RotateConstantly : MonoBehaviour
{
    [SerializeField] public float speed = 1f;


    void Update()
    {
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + Vector3.up * speed * Time.deltaTime * 10);
    }
}
