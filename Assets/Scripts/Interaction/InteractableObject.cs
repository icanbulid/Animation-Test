using UnityEngine;
using StarterAssets;

public class InteractableObject : MonoBehaviour
{
    [Tooltip("Текст, который будет отображаться при наведении на объект")]
    public string interactionText = "Interact [E]";

    // Дополнительные методы для взаимодействия
    public virtual void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
    }
}