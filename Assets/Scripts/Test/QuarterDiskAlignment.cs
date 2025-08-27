using System.Collections.Generic;
using UnityEngine;

public class QuarterDiskAlignment : MonoBehaviour
{
    [SerializeField] private Transform _rotateGreen; // 綠色 1/4 圓盤
    [SerializeField] private Transform _rotateRed;   // 紅色 1/4 圓盤
    [SerializeField] private Transform _rotateBlue;  // 藍色 1/4 圓盤

    private Transform _cameraTransform; // 相機的 Transform

    private enum AxisType { Red, Green, Blue } // 用於定義忽略的軸向

    private Dictionary<AxisType, float[]> _rotationMap = new Dictionary<AxisType, float[]>()
    {
        { AxisType.Green, new float[] { 90f, 180f, 0f, -90f } },
        { AxisType.Red, new float[] { -90f, 0f, 180f, 90f } },
        { AxisType.Blue, new float[] { -90f, 0f, 180f, 90f } }
    };

    private void Start()
    {
        _cameraTransform = Camera.main.transform; // 獲取主相機
    }

    private void Update()
    {
        // 控制綠色 1/4 圓盤（忽略 Y 軸高度）
        AlignQuarterDisk(_rotateGreen, AxisType.Green);

        // 控制紅色 1/4 圓盤（忽略 Z 軸高度）
        AlignQuarterDisk(_rotateRed, AxisType.Red);

        // 控制藍色 1/4 圓盤（忽略 X 軸高度）
        //AlignQuarterDisk(_rotateBlue, AxisType.Blue);
    }

    private void AlignQuarterDisk(Transform quarterDisk, AxisType axis)
    {
        // 檢查相機到圓盤的距離，避免重疊
        if (Vector3.Distance(_cameraTransform.position, quarterDisk.position) < 0.01f)
        {
            Debug.LogWarning("Camera is too close to the quarter disk.");
            return;
        }

        // 計算相機到圓盤的方向向量，忽略指定軸
        Vector3 directionToCamera = _cameraTransform.position - quarterDisk.position;
        directionToCamera = IgnoreAxis(directionToCamera, axis);
        directionToCamera = directionToCamera == Vector3.zero ? Vector3.forward : directionToCamera.normalized;

        // 計算圓盤的本地 Z 軸方向，忽略指定軸
        Vector3 diskForward = quarterDisk.forward;
        diskForward = IgnoreAxis(diskForward, axis);
        diskForward = diskForward == Vector3.zero ? Vector3.forward : diskForward.normalized;

        // 計算角度
        float angle = GetAngle(diskForward, directionToCamera, axis);

        // 調試輸出
        Debug.Log($"Angle: {angle}, Forward: {diskForward}, ToCamera: {directionToCamera}");

        // 將角度限制為最近的 0°, 90°, 180°, -90°
        float targetYRotation = GetClosestRotation(angle, axis);
        Debug.Log($"GetClosestRotation: {targetYRotation}");

        // 設置旋轉
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

    // 根據角度計算最接近的 0°, 90°, 180°, -90°
    private float GetClosestRotation(float angle, AxisType axis)
    {
        // 可用的角度列表
        float[] availableAngles = _rotationMap[axis];

        // 判斷角度範圍並返回索引
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
