using UnityEngine;

public class AdditionalCameraControlls : MonoBehaviour
{
    public float ySensitivity;

    private void FixedUpdate()
    {
        transform.Rotate(new Vector3(-Input.GetAxisRaw("Mouse Y") * 10.0f * Time.fixedDeltaTime * ySensitivity, 0, 0));
    }
}
