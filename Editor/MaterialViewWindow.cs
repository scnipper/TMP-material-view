using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        [SerializeField] private string defaultText = "Test text";
        [SerializeField] private TMP_Text useText;
        
        private Vector2 scrollPosition;
        
        private static readonly string RootPathSave = Path.Combine("Library","TMP_MaterialView"); 
        private static readonly List<Texture2D> textures = new();
        private static readonly int layer = LayerMask.GetMask("Water");
        private static string lastFontUse;

        private readonly Color clearColor = new(1f, 1f, 1f, 0f);
        private Material[] materialPresets;
        private string openPrefabPath;

        public static void ShowWindow(TMP_Text text)
        {
            var window = GetWindow<MaterialViewWindow>(true, "Material View");
            window.minSize = new Vector2(300, 400);
            window.maxSize = new Vector2(300, 400);
            window.useText = text;
            


            var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

            window.position = new Rect(mousePos.x, mousePos.y, 300, 400);
            
            window.UpdateMaterialPresets();

            window.RestoreTextures();
            window.ShowUtility();


        }


        private void RestoreTextures()
        {
	        if (Directory.Exists(RootPathSave))
	        {
		        if (lastFontUse != useText.font.name)
		        {
			        textures.Clear();

			        var directoryFont = Path.Combine(RootPathSave, useText.font.name);
		        
			        if (Directory.Exists(directoryFont))
			        {
				        foreach (var materialPreset in materialPresets)
				        {
					        var pathToPngImage = Path.Combine(directoryFont, materialPreset.name+".png");
					        var bytes = File.ReadAllBytes(pathToPngImage);

					        var texture2D = CreateEmpty2DTexture(1024, 1024);
				        
					        texture2D.LoadImage(bytes);

					        textures.Add(texture2D);
				        }

				        lastFontUse = useText.font.name;
			        }
		        }
	        }

        }

        private Material[] MaterialPresetsObtain(TMP_FontAsset fontAsset)
        {
	        var type = typeof(TMP_EditorUtility);

	        var methodInfo = type.GetMethod("FindMaterialReferences",BindingFlags.Static | BindingFlags.NonPublic);

	        return (Material[])methodInfo.Invoke(null, new []{fontAsset});
        }
        
        
        private void UpdateMaterialPresets()
        {
	        
	        materialPresets = MaterialPresetsObtain(useText.font);
        }

        private void DrawText()
        {
	        var fontName = useText.font.name;

	        var pathToFolder = Path.Combine(RootPathSave,fontName);
	        if (Directory.Exists(pathToFolder))
	        {
		        Directory.Delete(pathToFolder, true);
	        }
	        textures.Clear();

            var scene = StartEmptyScene();
            var camera = CreateCamera();
            
            var text = CreateText();
            
			UpdateMaterialPresets();
            foreach (var materialPreset in materialPresets)
            {
	            text.fontSharedMaterial = materialPreset;
	            var tex = GenerateTexture(camera);
	            SaveTexture(tex,materialPreset.name);
	            //var trimTexture = TrimTexture(tex);
	            textures.Add(tex);
	            
            }

            var cameraTargetTexture = camera.targetTexture;
            camera.targetTexture = null;
            DestroyImmediate(cameraTargetTexture);
            
            
            
			EditorSceneManager.CloseScene(scene,true);
			if (openPrefabPath != "")
			{
				PrefabStageUtility.OpenPrefab(openPrefabPath);
			}
        }

        private void SaveTexture(Texture2D texture, string materialPresetName)
        {
	        var fontName = useText.font.name;
	        var directoryFont = Path.Combine(RootPathSave,fontName);

	        Directory.CreateDirectory(directoryFont);

	        var pngBytes = texture.EncodeToPNG();
	        
	        File.WriteAllBytes(Path.Combine(directoryFont, $"{materialPresetName}.png"), pngBytes);
        }

        private Texture2D CreateEmpty2DTexture(int width, int height)
        {
	        return  new Texture2D(width, height, GraphicsFormat.R8G8B8A8_UNorm, TextureCreationFlags.None);
        }
        
        private Texture2D GenerateTexture(Camera camera)
        {
            RenderTexture.active = camera.targetTexture;

            var texture2D = CreateEmpty2DTexture(camera.targetTexture.width, camera.targetTexture.height);


            camera.Render();
            texture2D.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);

            texture2D.Apply();

            RenderTexture.active = null;

            return texture2D;
        }

        private TextMeshPro CreateText()
        {
            var goText = new GameObject("TEXT");

            goText.layer = LayerMask.NameToLayer("Water");
            
            var textMeshPro = goText.AddComponent<TextMeshPro>();

            
            var rectText = textMeshPro.GetComponent<RectTransform>();

            rectText.sizeDelta = new Vector2(10, 5);

            //textMeshPro.autoSizeTextContainer = true;
            textMeshPro.fontSizeMin = 2;
            textMeshPro.fontSize = 20;
            textMeshPro.font = useText.font;
            textMeshPro.text = defaultText;
            textMeshPro.alignment = TextAlignmentOptions.Center;

            return textMeshPro;
        }
        
        
        /// <summary>
        /// Open new scene
        /// </summary>
        private Scene StartEmptyScene()
        {
	        var currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
	        if (currentPrefabStage != null)
	        {
		        openPrefabPath = currentPrefabStage.assetPath;
	        }
	        else
	        {
		        openPrefabPath = "";
	        }
			
            var tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            tempScene.name = "Temp scene";
            
            return tempScene;
        }
        
        private Camera CreateCamera()
        {
            var cameraGO = new GameObject("camera");
            var camera = cameraGO.AddComponent<Camera>();
            camera.cullingMask = layer;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = clearColor;
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

            defaultText = EditorGUILayout.TextField("Text", defaultText);
            if (GUILayout.Button("Repaint text"))
            {
                DrawText();
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();
            for (var index = 0; index < textures.Count; index++)
            {
	            var texture2D = textures[index];

	            if(index >= materialPresets.Length) break;
	            
	            EditorGUILayout.BeginVertical("BOX");
	            EditorGUILayout.LabelField(materialPresets[index].name);
	            var controlRect = EditorGUILayout.GetControlRect(false, 60);
	            if (GUI.Button(controlRect,""))
	            {
		            useText.fontSharedMaterial = materialPresets[index];
		            useText.ForceMeshUpdate();
		            EditorUtility.SetDirty(useText);
		            Close();
	            }

	            controlRect.width += 120;
	            controlRect.height += 120;
	            controlRect.x -= 60;
	            controlRect.y -= 60;
	            GUI.DrawTexture(controlRect, texture2D, ScaleMode.ScaleToFit);
	            
	            EditorGUILayout.EndVertical();

            }

            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            
            GUILayout.EndVertical();
        }
        
        
        /// <summary>
        /// Trim texture
        /// </summary>
        /// <param name="texture2D"></param>
        /// <param name="vector2s"></param>
        private Texture2D TrimTexture(Texture2D textureIndex)
		{
			var pixels = textureIndex.GetPixels();


			int width = textureIndex.width;
			int height = textureIndex.height;

			int leftOffset = 0;
			int rightOffset = 0;
			int topOffset = 0;
			int bottomOffset = 0;

			List<int> valuesList = new List<int>();
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					var pixel = pixels[GetIndex(j, i, width)];
					if (pixel != clearColor)
					{
						int tmpOffset = j;
						valuesList.Add(tmpOffset);
					}
				}
			}

			if (valuesList.Count > 0)
			{
				leftOffset = valuesList.Min();
			}
			
			valuesList.Clear();

			for (int i = 0; i < height; i++)
			{
				for (int j = width - 1; j >= 0; j--)
				{
					var pixel = pixels[GetIndex(j, i, width)];
					if (pixel != clearColor)
					{
						int tmpOffset = j;

						valuesList.Add(tmpOffset);
					}
				}
			}

			if (valuesList.Count > 0)
			{
				rightOffset = valuesList.Max();
			}
			valuesList.Clear();

			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					var pixel = pixels[GetIndex(i, j, width)];
					if (pixel != clearColor)
					{
						int tmpOffset = j;

						valuesList.Add(tmpOffset);
						
					}
				}
			}
			
			if (valuesList.Count > 0)
			{
				topOffset = valuesList.Min();
			}
			valuesList.Clear();

			for (int i = 0; i < width; i++)
			{
				for (int j = height - 1; j >= 0; j--)
				{
					var pixel = pixels[GetIndex(i, j, width)];
					if (pixel != clearColor)
					{
						int tmpOffset = j;
						
						valuesList.Add(tmpOffset);
						
					}
				}
			}

			if (valuesList.Count > 0)
			{
				bottomOffset = valuesList.Max();
			}

			leftOffset -= 4;
			topOffset -= 4;

			if (leftOffset < 0) leftOffset = 0;
			if (topOffset < 0) topOffset = 0;

			rightOffset += 4;
			bottomOffset += 4;

			if (rightOffset > width)
			{
				rightOffset = width;
			}

			if (bottomOffset > height)
			{
				bottomOffset = height;
			}

			
			var rectTrueTexture = Rect.MinMaxRect(leftOffset, topOffset, rightOffset, bottomOffset);
			

			
			if (rectTrueTexture.width == 4 && rectTrueTexture.height == 4)
			{
				return null;
			} 
			var trueTexture = new Texture2D((int) rectTrueTexture.width, (int) rectTrueTexture.height,
											GraphicsFormat.R8G8B8A8_UNorm, TextureCreationFlags.None);
			
			Graphics.CopyTexture(textureIndex, 0, 0,
								(int) rectTrueTexture.x, (int) rectTrueTexture.y, (int) rectTrueTexture.width,
								(int) rectTrueTexture.height, trueTexture, 0,
								0, 0, 0);
			return trueTexture;
		}
        
        
		private int GetIndex(int x, int y, int dimension)
		{
			return x + y * dimension;
		}

        /// <summary>
        /// Method to retrieve the material presets that match the currently selected font asset.
        /// </summary>
        protected GUIContent[] GetMaterialPresets(TMP_FontAsset fontAsset)
        {
            var materialPresets = MaterialPresetsObtain(fontAsset);
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
        
    }
}