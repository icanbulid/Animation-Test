using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : InteractableObject
{
    public string nextLevelName;

    public override void Interact()
    {
        base.Interact();
        LoadNextLevel();
    }

    private void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            // Проверяем, существует ли сцена с таким именем
            if (Application.CanStreamedLevelBeLoaded(nextLevelName))
            {
                SceneManager.LoadScene(nextLevelName);
            }
            else
            {
                Debug.LogError($"Scene '{nextLevelName}' not found in Build Settings!");
            }
        }
        else
        {
            Debug.LogWarning("Next level name is not set!");
        }
    }
}