using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

namespace TMP_MaterialView.Editor
{
    [CustomEditor(typeof(TextMeshProUGUI), true), CanEditMultipleObjects]
    public class CustomTMP : TMP_EditorPanelUI
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Select material preset"))
            {
                MaterialViewWindow.ShowWindow(target as TMP_Text);
            }

            base.OnInspectorGUI();
        }
        
    }
}