using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;

public class CustomObjectGenerator : EditorWindow
{
    /// <summary>
    /// Name given for the custom object
    /// </summary>
    string _customObjectName;

    /// <summary>
    /// Category for the custom object
    /// </summary>
    string _customObjectCategory;

    /// <summary>
    /// GameObject that will be made as the custom object
    /// </summary>
    [SerializeField]
    GameObject _customObject;

    //Create the Window
    [MenuItem("Tools/Custom Object Generator")]
    public static void ShowWindow()
    {
        GetWindow<CustomObjectGenerator>("Custom Object Generator");
    }

    //Draw Window Elements
    public void OnGUI()
    {
        EditorGUILayout.LabelField("Enter name for Custom Object");
        _customObjectName = EditorGUILayout.TextField(_customObjectName);

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Enter file for Custom Object");
        _customObjectCategory = EditorGUILayout.TextField(_customObjectCategory);

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Enter Game Object you want to add as Custom Object");
        _customObject = (GameObject)EditorGUILayout.ObjectField(_customObject, typeof(GameObject), true);

        if(GUILayout.Button("Generate Custom Object"))
        {
            GenerateCustomObject();
        }
    }

    // Create the custom object reference and the script that will handle the menu item functionality
    void GenerateCustomObject()
    {
        string prefabPath = "Assets/CustomObjects/" + _customObjectName + ".prefab";
        string scriptPath = "Assets/Editor/CustomObjectScripts/" + _customObjectCategory + "_" + _customObjectName + ".cs";

        //Create the folder where the reference of the custom object will be stored if not existant and then save it in the folder
        if (!AssetDatabase.IsValidFolder("Assets/CustomObjects"))
        {
            Debug.Log(AssetDatabase.CreateFolder("Assets", "CustomObjects"));
        }
        PrefabUtility.SaveAsPrefabAsset(_customObject,prefabPath);

        //Create the class that will handle spawning the custom object from the menu item
        StringBuilder generatedCode = new StringBuilder();
        generatedCode.AppendLine("//This code was auto-generated by CustomObjectGenerator");
        generatedCode.AppendLine("");
        generatedCode.AppendLine("using System.Collections;");
        generatedCode.AppendLine("using System.Collections.Generic;");
        generatedCode.AppendLine("using UnityEngine;");
        generatedCode.AppendLine("using UnityEditor;");
        generatedCode.AppendLine("");
        generatedCode.AppendLine("public class " + _customObjectCategory + "_" + _customObjectName);
        generatedCode.AppendLine("{");
        generatedCode.AppendLine("  [MenuItem(\"GameObject/Custom Objects/" + _customObjectCategory + "/" + _customObjectName + "\", false,0)]");
        generatedCode.AppendLine("  static void Create" + _customObjectName + "()");
        generatedCode.AppendLine("  {");
        generatedCode.AppendLine("      GameObject obj = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath(\"" + prefabPath + "\", typeof(GameObject))) as GameObject;");
        generatedCode.AppendLine("      PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);");
        generatedCode.AppendLine("      Selection.activeGameObject = obj;");
        generatedCode.AppendLine("  }");
        generatedCode.AppendLine("}");

        //Create the folder where the generated script of the custom object will be stored
        if (!AssetDatabase.IsValidFolder("Assets/Editor/CustomObjectScripts"))
        {
            Debug.Log(AssetDatabase.CreateFolder("Assets/Editor", "CustomObjectScripts"));
        }

        //Delete old version and add the new created script
        File.Delete(scriptPath);
        File.WriteAllText(scriptPath, generatedCode.ToString(), Encoding.UTF8);
        AssetDatabase.ImportAsset(scriptPath);
        EditorUtility.DisplayDialog("Created", "Custom Object has been created.", "OK");
    }
}
