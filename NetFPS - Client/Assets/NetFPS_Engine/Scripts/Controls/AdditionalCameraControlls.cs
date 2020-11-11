using UnityEngine;

public class AdditionalCameraControlls : MonoBehaviour
{
    public float ySensitivity;

    private void Update()
    {
        transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y") * 10.0f * Time.deltaTime * ySensitivity, 0, 0));
    }
}
