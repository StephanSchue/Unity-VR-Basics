using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour {

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2, Phone = 3 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;
    float rotationY = 0F;

    private Vector3 lastMousePosition;

    void Update()
    {
        if(axes == RotationAxes.MouseXAndY)
        {
            float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
        }
        else if(axes == RotationAxes.MouseX)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
        }
        else if(axes == RotationAxes.Phone)
        {
            Vector3 heading = lastMousePosition - Input.mousePosition;
            Vector3 direction = heading / heading.magnitude;

            direction.x *= sensitivityX;
            direction.y *= sensitivityY;

            transform.localEulerAngles += direction;

            //float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

            //rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            //rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            //transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);

            lastMousePosition = Input.mousePosition;
        }
        else
        {
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
        }
    }

    void Start()
    {
        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }
}
