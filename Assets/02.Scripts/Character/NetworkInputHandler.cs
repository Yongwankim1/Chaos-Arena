using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkInputHandler : MonoBehaviour
{
    private StarterAssetsInputs _input;

    private void Awake()
    {
        _input = GetComponent<StarterAssetsInputs>();
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData data = new();

        data.Move = _input.move;
        data.Look = _input.look;
        data.Jump = _input.jump;
        data.Sprint = _input.sprint;

        _input.jump = false;

        return data;
    }
}