//using Unity.Cinemachine;
//using UnityEngine;

//public class CameraSwitcher : MonoBehaviour
//{
//    public CinemachineCamera freeLookCamera; // Свободная камера
//    public CinemachineCamera shoulderCamera; // Камера за плечом

//    private bool isAiming = false;

//    void Update()
//    {
//        if (Input.GetMouseButtonDown(1)) // ПКМ для прицеливания
//        {
//            isAiming = !isAiming;
//            SwitchCamera(isAiming);
//        }
//    }

//    public PlayerMovement playerController; // Ссылка на контроллер персонажа

//    void SwitchCamera(bool aim)
//    {
//        if (aim)
//        {
//            shoulderCamera.Priority = 10;
//            freeLookCamera.Priority = 0;
//            playerController.isAiming = true; // Включаем режим прицеливания
//        }
//        else
//        {
//            shoulderCamera.Priority = 0;
//            freeLookCamera.Priority = 10;
//            playerController.isAiming = false; // Выключаем режим прицеливания
//        }
//    }
//}