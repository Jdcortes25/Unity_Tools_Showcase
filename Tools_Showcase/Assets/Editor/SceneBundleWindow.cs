using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

public class SceneBundleWindow : EditorWindow
{
    /// <summary>
    /// Class to present on a reordable list the data needed to fill out the Scene Bundle
    /// </summary>
    [Serializable]
    public class SceneBundleData
    {
        public bool enabled = false;
        public SceneAsset scene = null;
    }

    /// <summary>
    /// List that the user fills out to add the scenes they want to include in the bundle
    /// </summary>
    ReorderableList inputedListOfSceneBundleData;

    /// <summary>
    /// List to store the data the user has entered in the ReordableList
    /// </summary>
    List<SceneBundleData> sceneBundleDataList = new List<SceneBundleData>();

    /// <summary>
    /// The scene bundle Asset that the user has entered in the window to possibly edit it
    /// </summary>
    [SerializeField]
    SceneBundleScriptObj inputedSceneBundleAsset;

    /// <summary>
    /// This will be used to check if the inputted asset is the same as the current one. If not then the fields in the window will be updated
    /// </summary>
    SceneBundleScriptObj currentSceneBundleAssetSelected;

    /// <summary>
    /// Asset name given by the user
    /// </summary>
    string inputedAssetName = string.Empty;

    /// <summary>
    /// Project name given by the user
    /// </summary>
    string inputedProjectName = string.Empty;

    /// <summary>
    /// Path to where should the Scene Bundle Assets should be saved
    /// </summary>
    const string filePathToSave = "Assets/Scenes/SceneBundles";

    [MenuItem("Tools/SceneBundle Window")]
    public static void ShowWindow()
    {
        GetWindow<SceneBundleWindow>("SceneBundle Window");
    }

    // Create the reordable list
    private void OnEnable()
    {
        inputedListOfSceneBundleData = new ReorderableList(sceneBundleDataList, typeof(SceneBundleData), true, true, true, true);

        inputedListOfSceneBundleData.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SceneBundleData sampleData = sceneBundleDataList[index];
                EditorGUI.BeginChangeCheck();
                sampleData.enabled = EditorGUI.Toggle(new Rect(rect.x, rect.y, 18, rect.height), sampleData.enabled);
                sampleData.scene = (SceneAsset)EditorGUI.ObjectField(new Rect(rect.x + 18, rect.y, rect.width - 18, rect.height), sampleData.scene, typeof(SceneAsset), true);
                EditorGUI.EndChangeCheck();
            };
    }

    // Populate the window
    private void OnGUI()
    {
        UpdateFields();
        EditorStyles.label.wordWrap = true;
        int spacing = 10;

        EditorGUILayout.LabelField("Insert here any previous scene bundle created if you wish to update it. If you wish to create a new one, leave this empty.");
        inputedSceneBundleAsset = (SceneBundleScriptObj)EditorGUILayout.ObjectField(inputedSceneBundleAsset, typeof(SceneBundleScriptObj), false);

        EditorGUILayout.Space(spacing);

        EditorGUILayout.LabelField("Enter a name for the Scene Bundle.");
        inputedAssetName = EditorGUILayout.TextField(inputedAssetName);

        EditorGUILayout.Space(spacing);

        EditorGUILayout.LabelField("Enter the name of the Project which this bundle belongs to.");
        inputedProjectName = EditorGUILayout.TextField(inputedProjectName);

        EditorGUILayout.Space(spacing);

        inputedListOfSceneBundleData.DoLayoutList();
        
        if(GUILayout.Button("Generate Scene Bundle"))
        {
            CreateSceneBundle();
        }
    }

    //If user has entered a bundle asset that is different from what the window was aware of, update all the fields in the window to match the new inputted asset.
    //If user has made the inputted asset empty then clear all the fields
    void UpdateFields()
    {
        if (inputedSceneBundleAsset != currentSceneBundleAssetSelected)
        {
            if (inputedSceneBundleAsset == null)
            {
                sceneBundleDataList.Clear();
                inputedAssetName = string.Empty;
                inputedProjectName = string.Empty;
            }
            else
            {
                inputedAssetName = inputedSceneBundleAsset.assetName;
                inputedProjectName = inputedSceneBundleAsset.projectName;
                List<SceneBundleData> tmpList = new List<SceneBundleData>();
                for (int i = 0; i < inputedSceneBundleAsset.scenes.Count; i++)
                {
                    SceneBundleData tmp = new SceneBundleData();
                    tmp.enabled = inputedSceneBundleAsset.scenes[i].isSceneEnabled;
                    tmp.scene = (SceneAsset)AssetDatabase.LoadAssetAtPath(inputedSceneBundleAsset.scenes[i].scenePath, typeof(SceneAsset));
                    tmpList.Add(tmp);
                }
                sceneBundleDataList = tmpList;
                inputedListOfSceneBundleData.list = tmpList;
            }
            currentSceneBundleAssetSelected = inputedSceneBundleAsset;
        }
    }

    //Create the Scriptable Object asset and generate the script responsible of adding menu items to load the scenes
    void CreateSceneBundle()
    {
        SceneBundleScriptObj newAsset = SceneBundleScriptObj.CreateInstance("SceneBundleScriptObj") as SceneBundleScriptObj;
        newAsset.assetName = inputedAssetName;
        newAsset.name = inputedAssetName;
        newAsset.projectName = inputedProjectName;
        List<SceneBundleData> tmpList = (List<SceneBundleData>)inputedListOfSceneBundleData.list;
        for (int i = 0; i < inputedListOfSceneBundleData.count; i++)
        {
            SceneBundle tmp = new SceneBundle(AssetDatabase.GetAssetPath(tmpList[i].scene), tmpList[i].enabled);
            newAsset.scenes.Add(tmp);
        }
        if (!AssetDatabase.IsValidFolder(filePathToSave))
        {
            Debug.Log(AssetDatabase.CreateFolder("Assets/Scenes", "SceneBundles"));

        }
        if(inputedSceneBundleAsset)
        {
            AssetDatabase.DeleteAsset(filePathToSave + "/" + inputedSceneBundleAsset.name + ".asset");
        }
        AssetDatabase.CreateAsset(newAsset, filePathToSave + "/" + newAsset.name + ".asset");
        EditorUtility.SetDirty(newAsset);
        AssetDatabase.SaveAssets();

        SceneBundleGenerator.GenerateSceneBundle();

        EditorUtility.DisplayDialog("Created", "Scene Bundle has been created.", "OK");
    }
}
