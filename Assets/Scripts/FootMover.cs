using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootMover : MonoBehaviour
{
    public Vector3 NewTarget { get; set; }


    [SerializeField] private Transform _targetPoint;
    [SerializeField] private float _distance;
    [SerializeField] private float _maxHeightDistance;


    [SerializeField] private float _countLerpPosition = 0.4f;
    [SerializeField] private float _countLerpHeight = 0.5f;
    [SerializeField] private float _speed = 5;
    [SerializeField] private float _amplutide = 0.4f;

    [SerializeField] private float _phaseOffset = 0f; // ����� �� ���� (��������, 0 ��� ������ ����, 0.5 ��� ������)


    private float currentTime = 1f;

    private void Start()
    {
        NewTarget = _targetPoint.position;
        currentTime = _phaseOffset; // ������������� ��������� ����� � ������ ������ �� ����
    }

    private void Update()
    {
        RaycastHit hit; // ����������, � ������� ������ ���������� � ��������� ����.

        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {

            if ((Vector3.Distance(hit.point, _targetPoint.position) > _distance)) // ��������� ���������� ����� ���������� �������� � ������� ��������.
            {

                currentTime = 0; // ���������� ����� �� 0
                NewTarget = hit.point; // ������ � �������� NewTarget  ���������� ����� ����� (��� ����� ����� ��� �����)
            }
            if (currentTime < 1)
            {
                // ��������� ����� �� ���� � �����������
                float phaseAdjustedTime = (currentTime + _phaseOffset) % 1f; // ������������ ����������� ����

                Vector3 footPosition = Vector3.Lerp(_targetPoint.position, hit.point, _countLerpPosition); // � ������� �������� ������������ ���������� ��������� �� ������� �����, � �����. 
                footPosition.y = Mathf.Lerp(footPosition.y, hit.point.y, _countLerpHeight) + (Mathf.Sin(phaseAdjustedTime * Mathf.PI) * _amplutide); // � ������� �������� ������������ � ������ ������ ����� ������ �����. 
                _targetPoint.position = footPosition; // ������ ������� ������� �� �����. 
                currentTime += Time.deltaTime * _speed;
            }

        }



    }


}