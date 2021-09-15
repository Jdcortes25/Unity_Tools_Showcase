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
    private class SceneBundleData
    {
        public bool enabled = false;
        public SceneAsset scene = null;
    }

    /// <summary>
    /// List that the user fills out to add the scenes they want to include in the bundle
    /// </summary>
    private ReorderableList _inputtedListOfSceneBundleData;

    /// <summary>
    /// List to store the data the user has entered in the ReordableList
    /// </summary>
    private List<SceneBundleData> _sceneBundleDataList = new List<SceneBundleData>();

    /// <summary>
    /// The scene bundle Asset that the user has entered in the window to possibly edit it
    /// </summary>
    [SerializeField]
    private SceneBundleScriptObj _inputtedSceneBundleAsset;

    /// <summary>
    /// This will be used to check if the inputted asset is the same as the current one. If not then the fields in the window will be updated
    /// </summary>
    private SceneBundleScriptObj _currentSceneBundleAssetSelected;

    /// <summary>
    /// Path to where should the Scene Bundle Assets should be saved
    /// </summary>
    private const string FILE_PATH_TO_SAVE = "Assets/Scenes/SceneBundles";

    /// <summary>
    /// Asset name given by the user
    /// </summary>
    private string _inputtedAssetName = string.Empty;

    /// <summary>
    /// Project name given by the user
    /// </summary>
    private string _inputtedProjectName = string.Empty;

    //Create the window
    [MenuItem("Tools/SceneBundle Window")]
    public static void ShowWindow()
    {
        GetWindow<SceneBundleWindow>("SceneBundle Window");
    }

    // Create the reordable list
    private void OnEnable()
    {
        _inputtedListOfSceneBundleData = new ReorderableList(_sceneBundleDataList, typeof(SceneBundleData), true, true, true, true);

        _inputtedListOfSceneBundleData.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SceneBundleData sampleData = _sceneBundleDataList[index];
                EditorGUI.BeginChangeCheck();
                sampleData.enabled = EditorGUI.Toggle(new Rect(rect.x, rect.y, 18, rect.height), sampleData.enabled);
                sampleData.scene = (SceneAsset)EditorGUI.ObjectField(new Rect(rect.x + 18, rect.y, rect.width - 18, rect.height), sampleData.scene, typeof(SceneAsset), true);
                EditorGUI.EndChangeCheck();
            };
    }

    // Populate the window
    private void OnGUI()
    {
        //Update the fields in the window if user changed the SceneBundle field
        UpdateFields();

        //Placing some settings for how to draw the GUI Elements
        EditorStyles.label.wordWrap = true;
        int spacing = 10;

        //Draw gui element for user to enter an existing scene bundle asset to update it
        EditorGUILayout.LabelField("Insert here any previous scene bundle created if you wish to update it. If you wish to create a new one, leave this empty.");
        _inputtedSceneBundleAsset = (SceneBundleScriptObj)EditorGUILayout.ObjectField(_inputtedSceneBundleAsset, typeof(SceneBundleScriptObj), false);

        EditorGUILayout.Space(spacing);

        //Draw gui element for user to enter the name for scene bundle asset
        EditorGUILayout.LabelField("Enter a name for the Scene Bundle.");
        _inputtedAssetName = EditorGUILayout.TextField(_inputtedAssetName);

        EditorGUILayout.Space(spacing);

        //Draw gui element for user to enter project name the scene bundle asset belongs
        EditorGUILayout.LabelField("Enter the name of the Project which this bundle belongs to.");
        _inputtedProjectName = EditorGUILayout.TextField(_inputtedProjectName);

        EditorGUILayout.Space(spacing);

        //Draw the Reordable list for user to enter the scenes they want to add
        EditorGUILayout.LabelField("Add the scene you want to add to the scene bundle. Check the box if you want to include the scene in your build setting");
        _inputtedListOfSceneBundleData.DoLayoutList();
        
        //Draw gui button for user to create the Scene Bundle
        if(GUILayout.Button("Generate Scene Bundle"))
        {
            CreateSceneBundle();
        }
    }

    //If user has entered a bundle asset that is different from what the window was aware of, update all the fields in the window to match the new inputted asset.
    //If user has made the inputted asset empty then clear all the fields
    void UpdateFields()
    {
        if (_inputtedSceneBundleAsset != _currentSceneBundleAssetSelected)
        {
            //Have all the fields in the window be empty if the scene bundle asset field is empty
            if (_inputtedSceneBundleAsset == null)
            {
                _sceneBundleDataList.Clear();
                _inputtedAssetName = string.Empty;
                _inputtedProjectName = string.Empty;
            }
            else
            {
                //Have all the fields in the window display their according data from the selected scene bundle asset
                _inputtedAssetName = _inputtedSceneBundleAsset.assetName;
                _inputtedProjectName = _inputtedSceneBundleAsset.projectName;
                List<SceneBundleData> tmpList = new List<SceneBundleData>();
                for (int i = 0; i < _inputtedSceneBundleAsset.scenes.Count; i++)
                {
                    SceneBundleData tmp = new SceneBundleData();
                    tmp.enabled = _inputtedSceneBundleAsset.scenes[i].isSceneEnabled;
                    tmp.scene = (SceneAsset)AssetDatabase.LoadAssetAtPath(_inputtedSceneBundleAsset.scenes[i].scenePath, typeof(SceneAsset));
                    tmpList.Add(tmp);
                }
                _sceneBundleDataList = tmpList;
                _inputtedListOfSceneBundleData.list = tmpList;
            }
            _currentSceneBundleAssetSelected = _inputtedSceneBundleAsset;
        }
    }

    //Create the Scriptable Object asset and generate the script responsible of adding menu items to load the scenes
    void CreateSceneBundle()
    {
        //Create the new Asset with the given information
        SceneBundleScriptObj newAsset = CreateInstance("SceneBundleScriptObj") as SceneBundleScriptObj;
        newAsset.assetName = _inputtedAssetName;
        newAsset.name = _inputtedAssetName;
        newAsset.projectName = _inputtedProjectName;
        List<SceneBundleData> tmpList = (List<SceneBundleData>)_inputtedListOfSceneBundleData.list;
        for (int i = 0; i < _inputtedListOfSceneBundleData.count; i++)
        {
            SceneBundle tmp = new SceneBundle(AssetDatabase.GetAssetPath(tmpList[i].scene), tmpList[i].enabled);
            newAsset.scenes.Add(tmp);
        }

        //Create folder for where to store the scene bundle asset if it does not exist
        if (!AssetDatabase.IsValidFolder(FILE_PATH_TO_SAVE))
        {
            Debug.Log(AssetDatabase.CreateFolder("Assets/Scenes", "SceneBundles"));

        }

        //Delete older version of the asset if this was an update of an existing one
        if (_inputtedSceneBundleAsset)
        {
            AssetDatabase.DeleteAsset(FILE_PATH_TO_SAVE + "/" + _inputtedSceneBundleAsset.name + ".asset");
        }

        //Save the new asset to the assigned folder
        AssetDatabase.CreateAsset(newAsset, FILE_PATH_TO_SAVE + "/" + newAsset.name + ".asset");
        EditorUtility.SetDirty(newAsset);
        AssetDatabase.SaveAssets();

        //Generate the scripts that will handle the menu items
        SceneBundleGenerator.GenerateSceneBundle();

        EditorUtility.DisplayDialog("Created", "Scene Bundle has been created.", "OK");
    }
}
