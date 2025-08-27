using System.Collections.Generic;
using UnityEngine;

public class QuarterDiskAlignment : MonoBehaviour
{
    [SerializeField] private Transform _rotateGreen; // ��� 1/4 ��L
    [SerializeField] private Transform _rotateRed;   // ���� 1/4 ��L
    [SerializeField] private Transform _rotateBlue;  // �Ŧ� 1/4 ��L

    private Transform _cameraTransform; // �۾��� Transform

    private enum AxisType { Red, Green, Blue } // �Ω�w�q�������b�V

    private Dictionary<AxisType, float[]> _rotationMap = new Dictionary<AxisType, float[]>()
    {
        { AxisType.Green, new float[] { 90f, 180f, 0f, -90f } },
        { AxisType.Red, new float[] { -90f, 0f, 180f, 90f } },
        { AxisType.Blue, new float[] { -90f, 0f, 180f, 90f } }
    };

    private void Start()
    {
        _cameraTransform = Camera.main.transform; // ����D�۾�
    }

    private void Update()
    {
        // ������ 1/4 ��L�]���� Y �b���ס^
        AlignQuarterDisk(_rotateGreen, AxisType.Green);

        // ������� 1/4 ��L�]���� Z �b���ס^
        AlignQuarterDisk(_rotateRed, AxisType.Red);

        // �����Ŧ� 1/4 ��L�]���� X �b���ס^
        //AlignQuarterDisk(_rotateBlue, AxisType.Blue);
    }

    private void AlignQuarterDisk(Transform quarterDisk, AxisType axis)
    {
        // �ˬd�۾����L���Z���A�קK���|
        if (Vector3.Distance(_cameraTransform.position, quarterDisk.position) < 0.01f)
        {
            Debug.LogWarning("Camera is too close to the quarter disk.");
            return;
        }

        // �p��۾����L����V�V�q�A�������w�b
        Vector3 directionToCamera = _cameraTransform.position - quarterDisk.position;
        directionToCamera = IgnoreAxis(directionToCamera, axis);
        directionToCamera = directionToCamera == Vector3.zero ? Vector3.forward : directionToCamera.normalized;

        // �p���L�����a Z �b��V�A�������w�b
        Vector3 diskForward = quarterDisk.forward;
        diskForward = IgnoreAxis(diskForward, axis);
        diskForward = diskForward == Vector3.zero ? Vector3.forward : diskForward.normalized;

        // �p�⨤��
        float angle = GetAngle(diskForward, directionToCamera, axis);

        // �ոտ�X
        Debug.Log($"Angle: {angle}, Forward: {diskForward}, ToCamera: {directionToCamera}");

        // �N���׭���̪� 0�X, 90�X, 180�X, -90�X
        float targetYRotation = GetClosestRotation(angle, axis);
        Debug.Log($"GetClosestRotation: {targetYRotation}");

        // �]�m����
        Vector3 currentEulerAngles = quarterDisk.eulerAngles;
        quarterDisk.eulerAngles = new Vector3(
            currentEulerAngles.x,
            axis == AxisType.Green ? targetYRotation : currentEulerAngles.y,
            axis == AxisType.Red || axis == AxisType.Blue ? targetYRotation : currentEulerAngles.z
        );
    }

    private float GetAngle(Vector3 diskForward, Vector3 directionToCamera, AxisType axis)
    {
        Vector3 origin = Vector3.zero;
        switch (axis)
        {
            case AxisType.Red:
                origin = Vector3.left;
                break;
            case AxisType.Green:
                origin = Vector3.up;
                break;
            case AxisType.Blue:
                origin = Vector3.forward;
                break;
        }
        return Vector3.SignedAngle(diskForward, directionToCamera, origin);
    }

    private Vector3 IgnoreAxis(Vector3 vector, AxisType axis)
    {
        switch (axis)
        {
            case AxisType.Red:
                vector.x = 0;
                break;
            case AxisType.Green:
                vector.y = 0;
                break;
            case AxisType.Blue:
                vector.z = 0;
                break;
        }
        return vector;
    }

    // �ھڨ��׭p��̱��� 0�X, 90�X, 180�X, -90�X
    private float GetClosestRotation(float angle, AxisType axis)
    {
        // �i�Ϊ����צC��
        float[] availableAngles = _rotationMap[axis];

        // �P�_���׽d��ê�^����
        if (angle >= 0f && angle < 90f) // 0 ~ 90
            return availableAngles[0];
        else if (angle >= 90f && angle < 180f) // 90 ~ 180
            return availableAngles[1];
        else if (angle >= -90f && angle < 0f) // -90 ~ 0
            return availableAngles[2];
        else // -180 ~ -90
            return availableAngles[3];
    }
}
