using UnityEngine;

public class SpitImpact : MonoBehaviour
{
    public GameObject bulletHolePrefab; // Префаб следа от пули
    public float holeSize = 0.1f; // Размер следа
    public float holeLifetime = 10f; // Время жизни следа

    void OnCollisionEnter(Collision collision)
    {
        // Получаем точку контакта
        ContactPoint contact = collision.contacts[0];

        // Создаем след от пули
        GameObject bulletHole = Instantiate(bulletHolePrefab, contact.point + contact.normal * 0.01f, Quaternion.identity);

        // Ориентируем след по нормали поверхности
        bulletHole.transform.forward = contact.normal;

        // Масштабируем след
        bulletHole.transform.localScale = new Vector3(holeSize, holeSize, holeSize);

        // Уничтожаем след через заданное время
        Destroy(bulletHole, holeLifetime);
    }
}
