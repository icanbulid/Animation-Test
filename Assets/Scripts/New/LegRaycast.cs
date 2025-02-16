using UnityEngine;

[System.Serializable]
public class LegRaycast : MonoBehaviour
{
    private RaycastHit hit;
    private Transform transform;

    public Vector3 Position => hit.point;
    public Vector3 Normal => hit.normal;

    private void Awake()
    {
        transform = base.transform;
    }

    private void Update()
    {
        var ray = new Ray(transform.position, -transform.up); // Направляем луч вниз
        Physics.Raycast(ray, out hit);

        // Визуализация луча
        Debug.DrawRay(transform.position, -transform.up * 10f, Color.red); // Рисуем луч длиной 10 единиц
    }
}