using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyMapSelector : NetworkBehaviour
{
    [SerializeField] private TMP_Dropdown mapDropdown;
    [SerializeField] private Image mapImage;
    [SerializeField] private Sprite[] mapSprites;

    private const string MapIndexPropertyKey = "mapIndex";

    private NetworkRunner runner;
    private int appliedMapIndex = -1;

    [Networked, OnChangedRender(nameof(OnMapChanged))]
    private int MapIndex { get; set; }

    private void OnEnable()
    {
        if (mapDropdown != null)
            mapDropdown.onValueChanged.AddListener(OnDropdownChanged);

        FindRunnerIfNeeded();
        RefreshInteractable();
        ApplyMap(GetSessionMapIndex(mapDropdown != null ? mapDropdown.value : 0));
    }

    private void OnDisable()
    {
        if (mapDropdown != null)
            mapDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    private void Update()
    {
        FindRunnerIfNeeded();
        RefreshInteractable();

        int sessionMapIndex = GetSessionMapIndex(appliedMapIndex);

        if (sessionMapIndex != appliedMapIndex)
            ApplyMap(sessionMapIndex);
    }

    public override void Spawned()
    {
        runner = Runner;
        RefreshInteractable();
        ApplyMap(MapIndex);
    }

    private void OnDropdownChanged(int index)
    {
        Debug.Log("Map dropdown changed: " + index);

        if (!IsHost())
            return;

        ApplyMap(index);
        UpdateSessionMapIndex(index);

        if (Object != null && Object.IsValid && HasStateAuthority)
        {
            MapIndex = index;
            return;
        }

        if (Object != null && Object.IsValid)
            RPC_RequestMapChange(index);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestMapChange(int index)
    {
        if (index < 0 || mapSprites == null || index >= mapSprites.Length)
            return;

        MapIndex = index;
        UpdateSessionMapIndex(index);
    }

    private void OnMapChanged()
    {
        ApplyMap(MapIndex);
    }

    private void ApplyMap(int index)
    {
        if (mapSprites == null || index < 0 || index >= mapSprites.Length)
            return;

        if (mapImage != null)
            mapImage.sprite = mapSprites[index];

        appliedMapIndex = index;

        if (mapDropdown != null && mapDropdown.value != index)
            mapDropdown.SetValueWithoutNotify(index);
    }

    private bool IsHost()
    {
        FindRunnerIfNeeded();

        return runner != null &&
            (runner.IsServer || runner.IsSharedModeMasterClient);
    }

    private void FindRunnerIfNeeded()
    {
        if (runner != null)
            return;

        runner = FindFirstObjectByType<NetworkRunner>();
    }

    private void RefreshInteractable()
    {
        if (mapDropdown == null)
            return;

        mapDropdown.interactable = IsHost();
    }

    private void UpdateSessionMapIndex(int index)
    {
        if (runner == null || runner.SessionInfo == null)
            return;

        runner.SessionInfo.UpdateCustomProperties(
            new Dictionary<string, SessionProperty>
            {
                { MapIndexPropertyKey, index }
            });
    }

    private int GetSessionMapIndex(int fallback)
    {
        if (runner == null || runner.SessionInfo == null)
            return fallback;

        IReadOnlyDictionary<string, SessionProperty> properties =
            runner.SessionInfo.Properties;

        if (properties == null)
            return fallback;

        if (!properties.TryGetValue(
                MapIndexPropertyKey,
                out SessionProperty property))
        {
            return fallback;
        }

        if (property.PropertyValue is int index)
            return index;

        if (property.PropertyValue != null &&
            int.TryParse(property.PropertyValue.ToString(), out index))
        {
            return index;
        }

        return fallback;
    }
}
