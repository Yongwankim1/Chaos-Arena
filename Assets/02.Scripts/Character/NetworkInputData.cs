using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 Move;
    public Vector2 Look;

    public NetworkBool Jump;
    public NetworkBool Sprint;
}