using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Transform focalPoint;
    private Vector3 lastMousePosition;
    public float rotationSpeed;
    [Min(0)] public float minXAngle = 0;
    public float maxXAngle = 90;

    private RedButton redButton;


    void Awake()
    {
        focalPoint = transform.parent;
        redButton = GameObject.Find("Red Button").GetComponent<RedButton>();
    }

    void Update()
    {
        focalPoint.position = redButton.currentPlayer.transform.position;
        if (Input.GetMouseButton(1))
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            transform.RotateAround(focalPoint.position, Vector3.up, mouseDelta.x * rotationSpeed * Time.deltaTime);
            transform.RotateAround(focalPoint.position, transform.right, Mathf.Clamp(-mouseDelta.y * rotationSpeed * Time.deltaTime, minXAngle - transform.rotation.eulerAngles.x, maxXAngle - transform.rotation.eulerAngles.x));
        }
        lastMousePosition = Input.mousePosition;
    }
}
