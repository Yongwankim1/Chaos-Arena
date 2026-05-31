using Fusion;
using UnityEngine;

// 퓨전 전용 입력 구조체 (NetworkInput)
public struct NetworkInputData : INetworkInput
{
    public Vector2 movementInput; // 이동 입력 (WASD)
    public NetworkBool isRuning; //달리기 여부
    public NetworkBool KillInput; //킬버튼 여부(F)
}