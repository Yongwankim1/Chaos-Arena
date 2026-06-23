using UnityEngine;

public class BruteBuffEffect : MonoBehaviour
{
    [SerializeField]
    private GameObject effectRoot;

    public void SetVisible(bool visible)
    {
        if (effectRoot != null)
        {
            effectRoot.SetActive(visible);
        }
    }
}