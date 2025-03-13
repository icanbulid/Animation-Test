//using UnityEngine;

//public class VectorEmitter : MonoBehaviour
//{
//    public GameObject targetObject; // Пустой объект, который будет перемещаться
//    public float maxDistance = 100f; // Максимальная дистанция

//    void Update()
//    {
//        // Получаем центр экрана
//        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

//        // Преобразуем координаты центра экрана в мировые координаты
//        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
//        RaycastHit hit;

//        // Проверяем, есть ли пересечение с коллайдером
//        if (Physics.Raycast(ray, out hit, maxDistance))
//        {
//            // Перемещаем пустой объект в точку пересечения
//            targetObject.transform.position = hit.point;

//            // Визуализация вектора (линии)
//            Debug.DrawLine(transform.position, hit.point, Color.red);
//        }
//        else
//        {
//            // Если нет пересечения, перемещаем объект на максимальную дистанцию
//            targetObject.transform.position = ray.GetPoint(maxDistance);

//            // Визуализация вектора (линии)
//            Debug.DrawLine(transform.position, ray.GetPoint(maxDistance), Color.green);
//        }
//    }
//}