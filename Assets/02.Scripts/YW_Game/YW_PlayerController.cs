using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class YW_PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 9f;

    private NetworkTransform _nt;

    [Networked] public Color PlayerColor {  get; set; }
    [Networked] public NetworkBool IsImposter {  get; set; }
    [Networked] public NetworkBool IsDead { get; set; }
    [Networked] public TickTimer KillTimer { get; set; }
    private MeshRenderer _meshRenderer;
    private ChangeDetector _changeDetector;

    private Color _lastAppliedColor;

    private NetworkCharacterController _controller;
    private void Awake()
    {
        _nt = GetComponent<NetworkTransform>();
        _meshRenderer = GetComponent<MeshRenderer>();
        //killCollTimer = GameObject.Find("KillCoolTimer").GetComponent<Image>();
        _controller = GetComponent<NetworkCharacterController>();
    }
    private void Update()
    {
        if(HasInputAuthority && IsDead)
        {
            bool requestedSpectate = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            requestedSpectate |= Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

            if (requestedSpectate)
            {
                StartSpectating();
            }
        }
    }
    public override void Spawned()
    {
        _lastAppliedColor = Color.clear;

        if (HasInputAuthority)
        {

            YW_CameraFollow camFollow = Camera.main.GetComponent<YW_CameraFollow>();
            if (camFollow != null)
            {
                camFollow.SetTarget(this.transform);
            }
        }

        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void Render()
    {
        if (_meshRenderer != null && PlayerColor != _lastAppliedColor)
        {
            _meshRenderer.material.color = PlayerColor;
            _lastAppliedColor = PlayerColor;
        }

        if (IsDead && _meshRenderer.enabled)
        {
            _meshRenderer.enabled = false;

            if (HasInputAuthority)
            {
                StartSpectating();
            }
        }
    }
    public override void FixedUpdateNetwork()
    {
        if (IsDead) return;
        if (YW_GameManager.Instance != null && YW_GameManager.Instance.IsGameOver) return;

        if (GetInput(out YW_NetworkInputData data))
        {
            Vector3 moveDirection = new Vector3(
                data.movementInput.x,
                0f,
                data.movementInput.y
            );

            if (moveDirection.sqrMagnitude > 1f)
                moveDirection.Normalize();

            float speed = data.isRuning ? runSpeed : moveSpeed;

            _controller.Move(moveDirection * speed);
        }
    }

    private void StartSpectating()
    {
        Debug.Log("관전 모드를 시작합니다");
        List<GameObject> allPlayer = new List<GameObject>();
        GameObject.FindGameObjectsWithTag("Player", allPlayer);

        List<YW_PlayerController> alivePlayers = new List<YW_PlayerController> ();

        foreach (var p in allPlayer)
        {
            if(!p.GetComponent<YW_PlayerController>().IsDead && p != this)
            {
                alivePlayers.Add(p.GetComponent<YW_PlayerController>());
            }
        }

        if(alivePlayers.Count > 0)
        {
            YW_PlayerController targetToSpectate = alivePlayers[Random.Range(0,alivePlayers.Count)];

            YW_CameraFollow camFollow = Camera.main.GetComponent<YW_CameraFollow>();
            if (camFollow != null)
            {
                camFollow.SetTarget(targetToSpectate.transform);
            }
        }
    }
}