using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyRotator : MonoBehaviour
{
    [SerializeField] private Transform _leftForwardFoot;
    [SerializeField] private Transform _leftBackFoot;
    [SerializeField] private Transform _rightForwardFoot;
    [SerializeField] private Transform _rightBackFoot;
    //[SerializeField] private Transform _centralBackFoot;
    [SerializeField] private float _rotateSpeed;


    [SerializeField] private float _koef;


    private float forwardRotation;
    private float lateralRotation;

    private void Update()
    {
        Rotate();

    }
    private void Rotate()
    {
        // �������� ������ ���� � ������� ������� ����� �������: �������� ������ - ������� ����� ��������� � ������� �������; ������� - ������� �����, ��������, �������� �������
        forwardRotation = (_leftForwardFoot.transform.position.y + _rightForwardFoot.transform.position.y) - (_leftBackFoot.transform.position.y + _rightBackFoot.transform.position.y);
        lateralRotation = ((_leftForwardFoot.transform.position.y + _leftBackFoot.transform.position.y) - (_rightForwardFoot.transform.position.y + _rightBackFoot.transform.position.y));
        if (Mathf.Abs(forwardRotation) < 1.5f) // ������� �� ��������� �������� �� ���������
        {
            forwardRotation = 0.0f;
        }
        if (Mathf.Abs(lateralRotation) < 1.5f)
        {
            lateralRotation = 0.0f;
        }




        Vector3 newRotation = new Vector3(forwardRotation * -_koef, transform.eulerAngles.y, lateralRotation * -_koef); //�������� ������� �� ����������. 

        float currentLateralRotationMoving = Mathf.LerpAngle(transform.eulerAngles.x, newRotation.x, _rotateSpeed); //���������� �������� ������� � ������� �������� ������������

        float currentForwardRotationMoving = Mathf.LerpAngle(transform.eulerAngles.z, newRotation.z, _rotateSpeed);

        transform.rotation = Quaternion.Euler(new Vector3(currentLateralRotationMoving, transform.eulerAngles.y, currentForwardRotationMoving));

    }


}