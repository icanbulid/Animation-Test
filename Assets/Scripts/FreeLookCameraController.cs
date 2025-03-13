using Unity.Cinemachine;
using UnityEngine;

public class FreeLookCameraController : MonoBehaviour
{
    public float sensitivity = 100f;

    private CinemachineFreeLook freeLookCamera;

    void Start()
    {
        freeLookCamera = GetComponent<CinemachineFreeLook>();
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        freeLookCamera.m_XAxis.Value += mouseX;
        freeLookCamera.m_YAxis.Value -= mouseY;
    }
}