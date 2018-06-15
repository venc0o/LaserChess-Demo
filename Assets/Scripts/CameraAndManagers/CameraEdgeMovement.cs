using UnityEngine;


public class CameraEdgeMovement : MonoBehaviour
{
    public int borderWidth = 10;
    public float moveSpeed = 20f;
    public float sensitivity = 3f;
    public float gravity = 3f;
    float smoothX;
    float smoothZ;

    void Update()
    {
        if (Input.mousePosition.x < 0f ||
           Input.mousePosition.x > Screen.width ||
           Input.mousePosition.y < 0f ||
           Input.mousePosition.y > Screen.height)
        {
            smoothX = Mathf.Lerp(smoothX, 0f, gravity * Time.deltaTime);
            smoothZ = Mathf.Lerp(smoothZ, 0f, gravity * Time.deltaTime);
            transform.Translate(smoothX * moveSpeed * Time.deltaTime, 0f, smoothZ * moveSpeed * Time.deltaTime);
            return;
        }

        if (Input.mousePosition.x < borderWidth)
        {
            smoothX = Mathf.Lerp(smoothX, -1f, sensitivity * Time.deltaTime);
        }
        else if (Input.mousePosition.x > Screen.width - borderWidth)
        {
            smoothX = Mathf.Lerp(smoothX, 1f, sensitivity * Time.deltaTime);
        }
        else
        {
            smoothX = Mathf.Lerp(smoothX, 0f, gravity * Time.deltaTime);
        }

        if (Input.mousePosition.y < borderWidth)
        {
            smoothZ = Mathf.Lerp(smoothZ, -1f, sensitivity * Time.deltaTime);
        }
        else if (Input.mousePosition.y > Screen.height - borderWidth)
        {
            smoothZ = Mathf.Lerp(smoothZ, 1f, sensitivity * Time.deltaTime);
        }
        else
        {
            smoothZ = Mathf.Lerp(smoothZ, 0f, gravity * Time.deltaTime);
        }

        transform.Translate(smoothX * moveSpeed * Time.deltaTime, 0f, smoothZ * moveSpeed * Time.deltaTime);
    }
}
