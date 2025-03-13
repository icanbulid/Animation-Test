using UnityEngine;
using StarterAssets;

public class AttackArea : MonoBehaviour
{
    public int damage = 10; // Урон, который наносит враг

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Получаем компонент ThirdPersonController у игрока и вызываем метод TakeDamage
            ThirdPersonController player = other.GetComponent<ThirdPersonController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }
}