using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;

public class BuildToolWindow : EditorWindow
{
    /// <summary>
    /// Custom Build Asset inputted by user
    /// </summary>
    [SerializeField]
    CustomBuildScriptObj _inputtedCustomBuild;

    /// <summary>
    /// Custom Build Asset currently in use
    /// </summary>
    CustomBuildScriptObj _selectedCustomBuild;

    /// <summary>
    /// Visual list where the user can add all the scene bundles they want for their custom build
    /// </summary>
    ReorderableList _inputedSceneBundleList;

    /// <summary>
    /// scene bundles that have been added by the user
    /// </summary>
    List<SceneBundleScriptObj> _selectedSceneBundles = new List<SceneBundleScriptObj>();

    /// <summary>
    /// File path for where the custom build asset will be saved
    /// </summary>
    const string FILE_PATH_TO_SAVE = "Assets/CustomBuilds";

    /// <summary>
    /// name for the custom build asset
    /// </summary>
    string _inputtedAssetName;

    /// <summary>
    /// File path where all the builds will be created
    /// </summary>
    string _inputtedFilePath;

    //Create the window
    [MenuItem("Tools/Build Tool Window")]
    public static void ShowWindow()
    {
        GetWindow<BuildToolWindow>("Build Tool Window");
    }

    //Create the visual list for user to add scene bundles
    void OnEnable()
    {
        _inputedSceneBundleList = new ReorderableList(_selectedSceneBundles, typeof(SceneBundleScriptObj), true, true, true, true);
        _inputedSceneBundleList.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                EditorGUI.BeginChangeCheck();
                _selectedSceneBundles[index] = (SceneBundleScriptObj)EditorGUI.ObjectField(new Rect(rect.x + 18, rect.y, rect.width - 18, rect.height), _selectedSceneBundles[index], typeof(SceneBundleScriptObj), false);
                EditorGUI.EndChangeCheck();
            };
    }

    //Draw all GUI Elements
    void OnGUI()
    {
        //Update the fields in the window if user has entered a Custom Build Asset
        UpdateFields();

        //Enter settings for how we want to draw the gui elements
        EditorStyles.label.wordWrap = true;
        int spacing = 10;

        //Draw gui element for user to enter an existing Custom Build Asset they want to modify
        EditorGUILayout.LabelField("Enter here a existing custom build asset or leave it empty to create a new one");
        _inputtedCustomBuild = (CustomBuildScriptObj)EditorGUILayout.ObjectField(_inputtedCustomBuild, typeof(CustomBuildScriptObj),false);

        EditorGUILayout.Space(spacing);

        //Draw gui element for user to enter a name for the Custom Build Asset
        EditorGUILayout.LabelField("Enter the name for the custo build asset");
        _inputtedAssetName = EditorGUILayout.TextField(_inputtedAssetName);

        EditorGUILayout.Space(spacing);

        //Draw gui element for user to enter a file path for where the build should be created.
        //A button will be provided for user to click which will summon the windos folder panel for user to browse and select where to make the build
        EditorGUILayout.LabelField("Enter file path where build should be made");
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("O"))
        {
            _inputtedFilePath = EditorUtility.OpenFolderPanel(string.Empty, string.Empty, string.Empty);
        }
        _inputtedFilePath = EditorGUILayout.TextField(_inputtedFilePath);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(spacing);

        //Draw the Reordable list where user can add all the scene bundles they want to include. A build will be made per scene bundle
        EditorGUILayout.LabelField("Add the scene bundles you want to build. A build will be made per scene bundle");
        _inputedSceneBundleList.DoLayoutList();

        EditorGUILayout.Space(spacing);

        //Draw gui button to save asset and create the buiold
        if(GUILayout.Button("Save and Build"))
        {
            if(_inputtedAssetName != string.Empty && _inputtedFilePath != null && _selectedSceneBundles.Count > 0)
            {
                SaveCustomBuildAsset();
                StartBuildProcess();
            }
            else
            {
                EditorUtility.DisplayDialog("ERROR", "One or more fields are empty", "OK");
            }           
        }
    }

    //Upate all the fields in the window if a existing custom build was entered, or clear everything if empty was selected
    void UpdateFields()
    {
        if(_inputtedCustomBuild != _selectedCustomBuild)
        {
            //Have all the fields in the window be empty if the selected Custom Build Asset is empty
            if(_inputtedCustomBuild == null)
            {
                _selectedSceneBundles = new List<SceneBundleScriptObj>();
                _inputedSceneBundleList.list = _selectedSceneBundles;
                _inputtedAssetName = string.Empty;
                _inputtedFilePath = string.Empty;
            }
            else
            {
                //Have all the fields in the window display their according data from the selected Custom Build Asset
                _inputtedAssetName = _inputtedCustomBuild.assetName;
                _inputtedFilePath = _inputtedCustomBuild.filePath;
                _selectedSceneBundles = _inputtedCustomBuild.sceneBundleList;
                _inputedSceneBundleList.list = _inputtedCustomBuild.sceneBundleList;
            }
            _selectedCustomBuild = _inputtedCustomBuild;
        }
    }

    //Save the data entered in the window to a custom build asset
    void SaveCustomBuildAsset()
    {
        //Create the new Custom Build Asset
        CustomBuildScriptObj newAsset = CreateInstance("CustomBuildScriptObj") as CustomBuildScriptObj;
        newAsset.assetName = _inputtedAssetName;
        newAsset.name = _inputtedAssetName;
        newAsset.filePath = _inputtedFilePath;
        newAsset.sceneBundleList = _selectedSceneBundles;

        //Create the folder to where the asset should be saved if it does not exist
        if (!AssetDatabase.IsValidFolder(FILE_PATH_TO_SAVE))
        {
            Debug.Log(AssetDatabase.CreateFolder("Assets", "CustomBuilds"));

        }

        //Delete the old version of this asset if this was being updated
        if (_inputtedCustomBuild)
        {
            AssetDatabase.DeleteAsset(FILE_PATH_TO_SAVE + "/" + _inputtedCustomBuild.name + ".asset");
        }

        //Save the new asset to the designated folder
        AssetDatabase.CreateAsset(newAsset, FILE_PATH_TO_SAVE + "/" + newAsset.name + ".asset");
        EditorUtility.SetDirty(newAsset);
        AssetDatabase.SaveAssets();
    }

    //Create all the builds listed
    void StartBuildProcess()
    {
        //Go through each entered SceneBundle asset, pass the scenes and build setting to a BuildPlayerOption, enter the file to place the build and create a build with the BuildPlayerOption
        if (Directory.Exists(_inputtedFilePath))
        {
            for(int i = 0; i < _selectedSceneBundles.Count; i++)
            {
                List<string> scenesToBuild = new List<string>();
                for(int j = 0; j < _selectedSceneBundles[i].scenes.Count; j++)
                {
                    if (_selectedSceneBundles[i].scenes[j].isSceneEnabled)
                    {
                        scenesToBuild.Add(_selectedSceneBundles[i].scenes[j].scenePath);
                    }
                }

                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.scenes = scenesToBuild.ToArray();
                buildPlayerOptions.target = BuildTarget.StandaloneWindows;
                Directory.CreateDirectory(_inputtedFilePath + "/" + _inputtedAssetName);
                Directory.CreateDirectory(_inputtedFilePath + "/" + _inputtedAssetName + "/" + _selectedSceneBundles[i].assetName);
                buildPlayerOptions.locationPathName = _inputtedFilePath + "/" + _inputtedAssetName + "/" + _selectedSceneBundles[i].assetName + "/" + _selectedSceneBundles[i].assetName + ".exe";
                BuildPipeline.BuildPlayer(buildPlayerOptions);
            }
            EditorUtility.DisplayDialog("DONE", "All builds have been made", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("ERROR", "File path could not be found", "OK");
        }
    }
}
