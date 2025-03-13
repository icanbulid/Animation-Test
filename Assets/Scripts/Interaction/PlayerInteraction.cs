using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Расстояние, на котором можно взаимодействовать с объектами")]
    public float interactionDistance = 3f;

    [Tooltip("Слой, на котором находятся объекты для взаимодействия")]
    public LayerMask interactionLayer;

    [Header("UI Elements")]
    [Tooltip("UI элемент для отображения текста взаимодействия")]
    public GameObject interactionUI;
    public TextMeshProUGUI interactionText;

    private Camera _mainCamera;
    private InteractableObject _currentInteractable;
    private StarterAssetsInputs _inputs;

    private void Start()
    {
        _mainCamera = Camera.main;
        _inputs = GetComponent<StarterAssetsInputs>();
        interactionUI.SetActive(false);
    }

    private void Update()
    {
        CheckForInteractable();
        HandleInteractionInput();
    }

    private void CheckForInteractable()
    {
        Ray ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                _currentInteractable = interactable;
                interactionText.text = interactable.interactionText;
                interactionUI.SetActive(true);
            }
            else
            {
                ClearInteractable();
            }
        }
        else
        {
            ClearInteractable();
        }
    }

    private void ClearInteractable()
    {
        if (_currentInteractable != null)
        {
            _currentInteractable = null;
            interactionUI.SetActive(false);
        }
    }

    private void HandleInteractionInput()
    {
        if (_currentInteractable != null && _inputs.interact)
        {
            _currentInteractable.Interact();
            _inputs.interact = false; // Сбрасываем флаг взаимодействия
        }
    }
}