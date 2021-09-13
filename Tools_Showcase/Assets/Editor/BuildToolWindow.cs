using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;

public class BuildToolWindow : EditorWindow
{
    /// <summary>
    /// Custom Build Asset inputed by user
    /// </summary>
    [SerializeField]
    CustomBuildScriptObj inputedCustomBuild;

    /// <summary>
    /// Custom Build Asset currently in use
    /// </summary>
    CustomBuildScriptObj selectedCustomBuild;

    /// <summary>
    /// name for the custom build asset
    /// </summary>
    string inputedAssetName;

    /// <summary>
    /// File path wheere all the builds will be created
    /// </summary>
    string inputedFilePath;

    /// <summary>
    /// File path for where the custom build asset will be saved
    /// </summary>
    const string filePathToSave = "Assets/CustomBuilds";

    /// <summary>
    /// Visual list where the user can add all the scene bundles they want for their custom build
    /// </summary>
    ReorderableList inputedSceneBundleList;

    /// <summary>
    /// scene bundles that have been added by the user
    /// </summary>
    List<SceneBundleScriptObj> selectedSceneBundles = new List<SceneBundleScriptObj>();

    //Get Window
    [MenuItem("Tools/Build Tool Window")]
    public static void ShowWindow()
    {
        GetWindow<BuildToolWindow>("Build Tool Window");
    }

    //Create the visual list for user to add scene bundles
    void OnEnable()
    {
        inputedSceneBundleList = new ReorderableList(selectedSceneBundles, typeof(SceneBundleScriptObj), true, true, true, true);
        inputedSceneBundleList.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                EditorGUI.BeginChangeCheck();
                selectedSceneBundles[index] = (SceneBundleScriptObj)EditorGUI.ObjectField(new Rect(rect.x + 18, rect.y, rect.width - 18, rect.height), selectedSceneBundles[index], typeof(SceneBundleScriptObj), false);
                EditorGUI.EndChangeCheck();
            };
    }

    //Draw all GUI Elements
    void OnGUI()
    {
        UpdateFields();
        EditorStyles.label.wordWrap = true;
        int spacing = 10;

        EditorGUILayout.LabelField("Enter here a existing custom build asset or leave it empty to create a new one");
        inputedCustomBuild = (CustomBuildScriptObj)EditorGUILayout.ObjectField(inputedCustomBuild, typeof(CustomBuildScriptObj),false);

        EditorGUILayout.Space(spacing);

        EditorGUILayout.LabelField("Enter the name for the custo build asset");
        inputedAssetName = EditorGUILayout.TextField(inputedAssetName);

        EditorGUILayout.Space(spacing);

        EditorGUILayout.LabelField("Enter file path where build should be made");
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("O"))
        {
            inputedFilePath = EditorUtility.OpenFolderPanel(string.Empty, string.Empty, string.Empty);
        }
        inputedFilePath = EditorGUILayout.TextField(inputedFilePath);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(spacing);

        EditorGUILayout.LabelField("Add the scene bundles you want to build. A build will be made per scene bundle");
        inputedSceneBundleList.DoLayoutList();

        EditorGUILayout.Space(spacing);

        if(GUILayout.Button("Save and Build"))
        {
            if(inputedAssetName != string.Empty && inputedFilePath != null && selectedSceneBundles.Count > 0)
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
        if(inputedCustomBuild != selectedCustomBuild)
        {
            if(inputedCustomBuild == null)
            {
                selectedSceneBundles = new List<SceneBundleScriptObj>();
                inputedSceneBundleList.list = selectedSceneBundles;
                inputedAssetName = string.Empty;
                inputedFilePath = string.Empty;
            }
            else
            {
                inputedAssetName = inputedCustomBuild.assetName;
                inputedFilePath = inputedCustomBuild.filePath;
                selectedSceneBundles = inputedCustomBuild.sceneBundleList;
                inputedSceneBundleList.list = inputedCustomBuild.sceneBundleList;
            }
            selectedCustomBuild = inputedCustomBuild;
        }
    }

    //Save the data entered in the window to a custom build asset
    void SaveCustomBuildAsset()
    {
        CustomBuildScriptObj newAsset = CreateInstance("CustomBuildScriptObj") as CustomBuildScriptObj;
        newAsset.assetName = inputedAssetName;
        newAsset.name = inputedAssetName;
        newAsset.filePath = inputedFilePath;
        newAsset.sceneBundleList = selectedSceneBundles;
        if (!AssetDatabase.IsValidFolder(filePathToSave))
        {
            Debug.Log(AssetDatabase.CreateFolder("Assets", "CustomBuilds"));

        }
        if (inputedCustomBuild)
        {
            AssetDatabase.DeleteAsset(filePathToSave + "/" + inputedCustomBuild.name + ".asset");
        }
        AssetDatabase.CreateAsset(newAsset, filePathToSave + "/" + newAsset.name + ".asset");
        EditorUtility.SetDirty(newAsset);
        AssetDatabase.SaveAssets();
    }

    //Create all the builds listed
    void StartBuildProcess()
    {
        if (Directory.Exists(inputedFilePath))
        {
            for(int i = 0; i < selectedSceneBundles.Count; i++)
            {
                List<string> scenesToBuild = new List<string>();
                for(int j = 0; j < selectedSceneBundles[i].scenes.Count; j++)
                {
                    if (selectedSceneBundles[i].scenes[j].isSceneEnabled)
                    {
                        scenesToBuild.Add(selectedSceneBundles[i].scenes[j].scenePath);
                    }
                }

                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.scenes = scenesToBuild.ToArray();
                buildPlayerOptions.target = BuildTarget.StandaloneWindows;
                Directory.CreateDirectory(inputedFilePath + "/" + inputedAssetName);
                Directory.CreateDirectory(inputedFilePath + "/" + inputedAssetName + "/" + selectedSceneBundles[i].assetName);
                buildPlayerOptions.locationPathName = inputedFilePath + "/" + inputedAssetName + "/" + selectedSceneBundles[i].assetName + "/" + selectedSceneBundles[i].assetName + ".exe";
                BuildPipeline.BuildPlayer(buildPlayerOptions);
            }
            EditorUtility.DisplayDialog("DONE", "All builds have been made", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("ERROR", "File path coudl not be found", "OK");
        }
    }
}
