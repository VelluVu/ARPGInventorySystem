using ARPGInventory;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemVisualManager))]
public class ItemVisualManagerEditor : Editor
{
    public const string AddListenersButtonLabel = "Add Listeners";
    public const string RemoveListenersButtonLabel = "Remove Listeners";

    public override void OnInspectorGUI()
    {
        var visualManager = (ItemVisualManager)target;
        base.OnInspectorGUI();
        if (GUILayout.Button(AddListenersButtonLabel))
        {
            visualManager.AddListeners();
        }
        if(GUILayout.Button(RemoveListenersButtonLabel))
        {
            visualManager.RemoveListeners();
        }
        if(GUILayout.Button("Clear UI Items"))
        {
            visualManager.ClearItemVisuals();
        }
    }
}
