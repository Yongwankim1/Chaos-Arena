using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapImageChanger : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] Sprite[] mapSprites = new Sprite[0];
    [SerializeField] Image mapImage;

    private void OnEnable()
    {
        if(dropdown != null)
            dropdown.onValueChanged.AddListener(MapImageChange);
        DefaultImage();
    }

    private void OnDisable()
    {
        if(dropdown != null )
            dropdown.onValueChanged.RemoveAllListeners();
    }

    private void MapImageChange(int index)
    {
        if(mapSprites.Length <= 0 || mapSprites[index] == null) return;

        mapImage.sprite = mapSprites[index];
    }

    private void DefaultImage()
    {
        if(dropdown.value > mapSprites.Length)
        {
            dropdown.value = mapSprites.Length - 1;
        }
        mapImage.sprite = mapSprites[dropdown.value];
    }
}
