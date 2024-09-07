using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;

namespace TMP_MaterialView.Editor
{
    public class MaterialViewWindow : EditorWindow
    {
        public static void ShowWindow(TMP_Text text)
        {
            var window = GetWindow<MaterialViewWindow>(true, "Material View");
            window.minSize = new Vector2(300, 400);
            window.maxSize = new Vector2(300, 400);

            

            var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

            window.position = new Rect(mousePos.x, mousePos.y, 300, 400);
            

            window.ShowUtility();


        }

        private void DrawText()
        {
            
        }
        
        /// <summary>
        /// Open new scene
        /// </summary>
        private Scene StartEmptyScene()
        {
            var tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            tempScene.name = "Temp scene";
            
            return tempScene;
        }
        
        private Camera CreateCamera()
        {
            var cameraGO = new GameObject("camera");
            var camera = cameraGO.AddComponent<Camera>();
            camera.cullingMask = LayerMask.GetMask("UI");
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(1, 1, 1, 0);
            camera.orthographic = true;
            
            var renderTexture = new RenderTexture(1024, 1024, 0, GraphicsFormat.R8G8B8A8_UNorm)
            {
                antiAliasing = 8
            };
            camera.targetTexture = renderTexture;

            camera.orthographicSize = 5.12f;
            camera.rect = new Rect(0, 0, 10.24f, 10.24f);


            cameraGO.transform.position = new Vector3(0, 0, -1);
            return camera;
        }


        private void OnLostFocus()
        {
            Close();
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.Button("LOL");
            if (GUILayout.Button("fuck"))
            {
                Close();
            }
            
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Method to retrieve the material presets that match the currently selected font asset.
        /// </summary>
        protected GUIContent[] GetMaterialPresets(TMP_FontAsset fontAsset)
        {
            var materialPresets = TMP_EditorUtility.FindMaterialReferences(fontAsset);
            var presetNames = new GUIContent[materialPresets.Length];

            //m_MaterialPresetIndexLookup.Clear();

            for (int i = 0; i < presetNames.Length; i++)
            {
                presetNames[i] = new GUIContent(materialPresets[i].name);

                //m_MaterialPresetIndexLookup.Add(materialPresets[i].GetInstanceID(), i);

                //if (m_TargetMaterial.GetInstanceID() == m_MaterialPresets[i].GetInstanceID())
                //    m_MaterialPresetSelectionIndex = i;
            }

            //m_IsPresetListDirty = false;

            return presetNames;
        }

        private void CreateGUI()
        {
            
        }
    }
}