using UnityEngine;

public interface IDash
{
    bool IsDashing { get; }

    Vector3 DashDirection { get; }

    void Dash();

    float GetMoveThisTick();
}