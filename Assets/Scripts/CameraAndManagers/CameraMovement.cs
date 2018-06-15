using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform Target;

    public float cameraDistance = 10f;
    public float zoomAngle = 30f;
    public float zoomRatio = 4f;
    public int zoomSteps = 4;
    public float zoomSensitivity = 2f;
    public int zoom = 2;

    public float moveSpeed = 20f;
    public float sensitivity = 3f;
    public float gravity = 3f;

    public bool isRotating;

    float smoothX;
    float smoothZ;
    float smoothAngle;
    float smoothHeight;
    float smoothOffset;
    float smoothRotAngle;

    readonly float topDownAngle = 90f;

    Transform parent;
    Transform child;

    float angle;
    float rotangle;



    void Awake()
    {
        parent = transform;
        child = parent.GetChild(0);
        smoothAngle = topDownAngle;
        smoothHeight = cameraDistance;
    }



    void Update()
    {


        float y = Input.GetAxis("Mouse ScrollWheel");

        zoom = Mathf.Clamp(zoom + (int)y, 0, zoomSteps - 1);


        float offset = 1;

        smoothOffset = Mathf.Lerp(smoothOffset, offset, zoomSensitivity * Time.deltaTime);

        float angleStep = (topDownAngle - zoomAngle) / (zoomSteps - 1);
        float angle = topDownAngle - angleStep * zoom;

        smoothAngle = Mathf.Lerp(smoothAngle, angle, zoomSensitivity * Time.deltaTime);

        child.localRotation = Quaternion.AngleAxis(smoothAngle, Vector3.right);

        float heightStep = (cameraDistance - cameraDistance / zoomRatio) / (zoomSteps - 1);
        float height = cameraDistance - heightStep * zoom;



        smoothHeight = Mathf.Lerp(smoothHeight, height, zoomSensitivity * Time.deltaTime);

        parent.localPosition = new Vector3(parent.localPosition.x, Mathf.Sin(smoothAngle * Mathf.Deg2Rad) * smoothHeight + smoothOffset, parent.localPosition.z);

        child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, -Mathf.Cos(smoothAngle * Mathf.Deg2Rad) * smoothHeight);



        if (Input.GetKey(KeyCode.E) || isRotating)
        {
            rotangle = Mathf.Lerp(rotangle, 20 * 3.6f, Time.deltaTime * 5);
            transform.RotateAround(transform.position + transform.forward, transform.up, rotangle * Time.deltaTime);
            Target = null;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            rotangle = Mathf.Lerp(rotangle, -20 * 3.6f, Time.deltaTime * 5);
            transform.RotateAround(transform.position + transform.forward, transform.up, rotangle * Time.deltaTime);
            Target = null;
        }
        else
            rotangle = Mathf.Lerp(rotangle, 0, Time.deltaTime * 5);


        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        if (x == 0f)
        {
            smoothX = Mathf.Lerp(smoothX, 0f, gravity * Time.deltaTime);
        }
        else
        {
            smoothX = Mathf.Lerp(smoothX, x, sensitivity * Time.deltaTime);
        }

        if (z == 0f)
        {
            smoothZ = Mathf.Lerp(smoothZ, 0f, gravity * Time.deltaTime);
        }
        else
        {
            smoothZ = Mathf.Lerp(smoothZ, z, sensitivity * Time.deltaTime);
        }

        if (Target != null && x == 0 && z == 0)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(Target.position.x, transform.position.y, Target.position.z) - transform.forward * 2, Time.deltaTime * 2);
        }
        else
        {
            Target = null;
            transform.Translate(smoothX * moveSpeed * Time.deltaTime, 0f, smoothZ * moveSpeed * Time.deltaTime);
        }

        Vector3 Restrpos = transform.position;
        Restrpos.x = Mathf.Clamp(transform.position.x, -5.0f, 10.0f);
        Restrpos.z = Mathf.Clamp(transform.position.z, -5.0f, 10.0f);
        transform.position = Restrpos;
    }



}