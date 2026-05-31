using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 9f;
    [SerializeField] private float killRange = 1.5f;
    [SerializeField] private TextMeshProUGUI myJob;
    [SerializeField] private float KillCoolDownTime = 10f;
    [SerializeField] private Image killCollTimer;

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
            GameObject uiTextObject = GameObject.Find("JobText");

            if (uiTextObject != null)
            {
                myJob = uiTextObject.GetComponent<TextMeshProUGUI>();
            }

            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.SetTarget(this.transform);
            }
        }

        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }
    private void UpdateJobUI()
    {
        if (!HasInputAuthority) return;
        if (myJob == null) return;

        if (IsImposter)
        {
            myJob.text = "Imposter";
            myJob.color = Color.red;
        }
        else
        {
            myJob.text = "Crew";
            myJob.color = Color.white;
        }
    }
    public override void Render()
    {
        if (_meshRenderer != null && PlayerColor != _lastAppliedColor)
        {
            _meshRenderer.material.color = PlayerColor;
            _lastAppliedColor = PlayerColor;
        }

        UpdateJobUI();

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
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        if (GetInput(out NetworkInputData data))
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



    private void TryKillCrewmate()
    {
        
        foreach(var otherPlayer in FindObjectsOfType<PlayerController>())
        {
            if (otherPlayer == this || otherPlayer.IsDead || otherPlayer.IsImposter) continue;

            float distance = Vector3.Distance(transform.position, otherPlayer.transform.position);

            if (distance <= killRange)
            {
                otherPlayer.IsDead = true;
                KillTimer = TickTimer.CreateFromSeconds(Runner,KillCoolDownTime);
                Debug.Log($"서버 판정: Player {otherPlayer.Object.InputAuthority}가 죽었습니다");
                break;
            }
        }
    }
    private void StartSpectating()
    {
        Debug.Log("관전 모드를 시작합니다");
        List<GameObject> allPlayer = new List<GameObject>();
        GameObject.FindGameObjectsWithTag("Player", allPlayer);

        List<PlayerController> alivePlayers = new List<PlayerController> ();

        foreach (var p in allPlayer)
        {
            if(!p.GetComponent<PlayerController>().IsDead && p != this)
            {
                alivePlayers.Add(p.GetComponent<PlayerController>());
            }
        }

        if(alivePlayers.Count > 0)
        {
            PlayerController targetToSpectate = alivePlayers[Random.Range(0,alivePlayers.Count)];

            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.SetTarget(targetToSpectate.transform);
            }
        }
    }
}