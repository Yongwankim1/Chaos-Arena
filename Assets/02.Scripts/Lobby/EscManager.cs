using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EscManager : MonoBehaviour
{
    public static EscManager Instance;
    private Stack<GameObject> openPanels = new Stack<GameObject>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ClosePanel();
        }
    }

    public void ClearStack()
    {
        openPanels.Clear();
    }

    public void OpenPanel(GameObject panel)
    {
        openPanels.Push(panel);

        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (openPanels.Count <= 0) return;
        GameObject panel = openPanels.Pop();

        panel.SetActive(false);
    }
}
