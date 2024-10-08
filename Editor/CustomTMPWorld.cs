using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

namespace TMP_MaterialView.Editor
{
    [CustomEditor(typeof(TextMeshPro), true), CanEditMultipleObjects]
    public class CustomTMPWorld : TMP_EditorPanel
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