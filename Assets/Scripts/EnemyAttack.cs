using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public GameObject attackHitbox; // Ссылка на пустой объект (например, AttackHitbox)

    // Метод для активации хитбокса
    public void ActivateHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
            AudioManager.instance.Play("Dragon_Bite");
        }
        
    }

    // Метод для деактивации хитбокса
    public void DeactivateHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }
}