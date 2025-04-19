using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameObjectRenderer : EditorWindow
{
    private GameObject objectToRender;
    private int resolution = 512;
    private string savePath = "";

    private Camera renderCamera;
    private string lastOpenedScene;
    private bool isPreviewScene = false;

    [MenuItem("Tools/GameObject Renderer")]
    public static void ShowWindow()
    {
        GetWindow<GameObjectRenderer>("GameObject Renderer");
    }

    private void OnEnable()
    {
        if (EditorPrefs.HasKey("resolution"))
        {
            resolution = EditorPrefs.GetInt("resolution");
        }
    }

    private void OnGUI()
    {
        //Resolution
        if (resolution <= 0)
        {
            resolution = 1;
        }
        else if (resolution > 8192)
        {
            resolution = 8192;
        }

        if (resolution != EditorPrefs.GetInt("resolution", -1))
        {
            EditorPrefs.SetInt("resolution", resolution);
        }

        //Window Setter
        GUILayout.Label("GameObject Renderer", EditorStyles.boldLabel);

        objectToRender = (GameObject) EditorGUILayout.ObjectField("Object to Render", objectToRender, typeof(GameObject), true);
        resolution = EditorGUILayout.IntField("Resolution", resolution);

        if (GUILayout.Button("Create Preview"))
        {
            if (objectToRender == null)
            {
                Debug.LogWarning("No object selected for rendering.");
                return;
            }
            if (isPreviewScene)
            {
                Debug.LogWarning("You are already in Render Scene!");
                return;
            }

            CreatePreviewScene();
        }

        if (isPreviewScene && GUILayout.Button("Save Preview..."))
        {
            SavePreview();
        }

        if (isPreviewScene && EditorSceneManager.GetActiveScene().name != "Preview Render Scene")
        {
            OnEnd(false);
        }
    }

    private void CreatePreviewScene()
    {
        if (objectToRender.GetComponentInChildren<Renderer>() == null)
        {
            Debug.LogWarning($"No renderers found in {objectToRender.name} or its children.");
            return;
        }

        lastOpenedScene = EditorSceneManager.GetActiveScene().path;

        GameObject clonedObject = PrefabUtility.SaveAsPrefabAsset(objectToRender, $"Assets/{objectToRender.name}_temp.prefab");
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
        renderCamera = FindFirstObjectByType<Camera>();
        objectToRender = Instantiate(clonedObject);
        AssetDatabase.DeleteAsset(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(clonedObject));

        scene.name = "Preview Render Scene";
        renderCamera.name = "Render Camera";
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = Color.clear;
        objectToRender.transform.position = Vector3.zero;
        objectToRender.name = objectToRender.name.Replace("_temp(Clone)", "");

        isPreviewScene = true;

        Debug.Log("Preview created successfully. Make the required changes in the opened scene.");
    }

    private void SavePreview()
    {
        if (EditorPrefs.HasKey("savePath")) 
        {
            savePath = EditorPrefs.GetString("savePath");
        }
        savePath = EditorUtility.OpenFolderPanel("Select Folder to Save PNG", savePath, "");

        if (string.IsNullOrEmpty(savePath)) return;

        RenderTexture renderTexture = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32);
        renderTexture.enableRandomWrite = true;
        RenderTexture.active = renderTexture;
        renderCamera.targetTexture = renderTexture;
        renderCamera.Render();

        Texture2D screenshot = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        screenshot.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        screenshot.Apply();

        string filePath = Path.Combine(savePath, $"{objectToRender.name}_Preview.png");
        byte[] pngData = screenshot.EncodeToJPG(); //Заменить потом обратно на EncodeToPNG()
        File.WriteAllBytes(filePath, pngData);

        EditorPrefs.SetString("savePath", savePath);
        RenderTexture.active = null;
        OnEnd(true);

        Debug.Log($"Preview saved at {filePath}");
    }

    private void OnEnd(bool willChangeScene)
    {
        isPreviewScene = false;
        renderCamera = null;
        objectToRender = null;
        if (!string.IsNullOrEmpty(lastOpenedScene) && willChangeScene)
        {
            EditorSceneManager.OpenScene(lastOpenedScene);
        }
    }
}
