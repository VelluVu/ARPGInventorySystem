using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class InventoryUI : MonoBehaviour
{
    private bool _isDebugging = false;
    public bool isDebugging = false;
    public static bool IsDebugging = false;

    private void OnValidate()
    {
        SetIsDebugging();
    }

    private void SetIsDebugging()
    {
        if (isDebugging == _isDebugging) return;
        _isDebugging = isDebugging;
        IsDebugging = _isDebugging;
    }
}
