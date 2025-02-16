using UnityEngine;

public class AddCollidersToBones : MonoBehaviour
{
    [ContextMenu("AddColliders")]
    public void AddColliders()
    {
        // �������� ��� ����� � ������
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer �� ������!");
            return;
        }

        Transform[] bones = skinnedMeshRenderer.bones;

        // ��������� ���������� �� ������ �����
        foreach (Transform bone in bones)
        {
            CapsuleCollider collider = bone.gameObject.AddComponent<CapsuleCollider>();
            // ��������� ���������� (����� �������� ��� ���� �����)
            collider.radius = 0.1f;
            collider.height = 0.5f;
            collider.direction = 2; // ��� Z
        }

        Debug.Log("���������� ��������� �� ��� �����.");
    }
    [ContextMenu("Say message for DevTribe")]
    void SayMessage()
    {
        Debug.Log("I love you, DevTribe");
    }
}
