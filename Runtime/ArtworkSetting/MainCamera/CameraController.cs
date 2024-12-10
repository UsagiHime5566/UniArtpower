using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float lookSpeedH = 2f;

    [SerializeField]
    private float lookSpeedV = 2f;

    [SerializeField]
    private float zoomSpeed = 2f;

    [SerializeField]
    private float dragSpeed = 3f;

    private float yaw = 0f;
    private float pitch = 0f;

    private async void Start()
    {
        //Wait for helper initialize
        await Task.Yield();

        // Initialize the correct initial rotation
        this.yaw = this.transform.eulerAngles.y;
        this.pitch = this.transform.eulerAngles.x;
    }

    public void ResetCoord(){
        this.yaw = this.transform.eulerAngles.y;
        this.pitch = this.transform.eulerAngles.x;
    }

    private void Update()
    {
        // Only work not in Setting Panel
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            //Look around with Left Mouse
            if (Input.GetMouseButton(0))
            {
                this.yaw += this.lookSpeedH * Input.GetAxis("Mouse X");
                this.pitch -= this.lookSpeedV * Input.GetAxis("Mouse Y");

                this.transform.eulerAngles = new Vector3(this.pitch, this.yaw, 0f);
            }

            //drag camera around with Middle Mouse
            if (Input.GetMouseButton(2))
            {
                this.transform.Translate(-Input.GetAxisRaw("Mouse X") * dragSpeed * .07f, -Input.GetAxisRaw("Mouse Y") * dragSpeed * .07f, 0);
            }

            if (Input.GetMouseButton(1))
            {
                //Zoom in and out with Right Mouse
                this.transform.Translate(0, 0, Input.GetAxisRaw("Mouse X") * this.zoomSpeed * .07f, Space.Self);
            }

            //Zoom in and out with Mouse Wheel
            this.transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * this.zoomSpeed, Space.Self);
        }
    }
}