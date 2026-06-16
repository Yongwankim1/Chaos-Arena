using Fusion;
using UnityEngine;
public struct NetworkInputData : INetworkInput
{
    public Vector2 Move;
    public Vector2 Look;

    public float Yaw;

    public NetworkBool Jump;
    public NetworkBool Sprint;

    public NetworkBool Attack;

    public NetworkBool Dash;
    public NetworkBool SkillQ;
    public NetworkBool SkillE;
}