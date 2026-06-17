using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    [SerializeField]
    private Material normalMaterial;

    [SerializeField]
    private Material ultimateMaterial;

    [SerializeField]
    private Material stealthMaterial;

    private bool _ultimate;
    private bool _stealth;

    private SkinnedMeshRenderer[] _renderers;
    private Material[] _originalMaterials;

    private void Awake()
    {
        _renderers =
            GetComponentsInChildren<SkinnedMeshRenderer>(true);

        _originalMaterials =
            new Material[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalMaterials[i] =
                _renderers[i].material;
        }
    }

    public void SetUltimate(bool value)
    {
        _ultimate = value;

        Refresh();
    }

    public void SetStealth(bool value)
    {
        _stealth = value;

        Refresh();
    }

    private void Refresh()
    {
        Material mat = null;

        if (_stealth)
        {
            mat = stealthMaterial;
        }
        else if (_ultimate)
        {
            mat = ultimateMaterial;
        }

        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].material =
                mat ?? _originalMaterials[i];
        }
    }
}