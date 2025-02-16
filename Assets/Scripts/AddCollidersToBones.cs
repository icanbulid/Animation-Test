using UnityEngine;

public class AddCollidersToBones : MonoBehaviour
{
    [ContextMenu("AddColliders")]
    public void AddColliders()
    {
        // Получаем все кости в модели
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer не найден!");
            return;
        }

        Transform[] bones = skinnedMeshRenderer.bones;

        // Добавляем коллайдеры на каждую кость
        foreach (Transform bone in bones)
        {
            CapsuleCollider collider = bone.gameObject.AddComponent<CapsuleCollider>();
            // Настройка коллайдера (можно изменить под ваши нужды)
            collider.radius = 0.1f;
            collider.height = 0.5f;
            collider.direction = 2; // Ось Z
        }

        Debug.Log("Коллайдеры добавлены на все кости.");
    }
    [ContextMenu("Say message for DevTribe")]
    void SayMessage()
    {
        Debug.Log("I love you, DevTribe");
    }
}
