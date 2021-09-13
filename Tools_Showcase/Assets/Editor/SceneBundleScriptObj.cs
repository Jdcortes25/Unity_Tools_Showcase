using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public class SceneBundleScriptObj : ScriptableObject
{
    /// <summary>
    /// name given to this scriptable object asset
    /// </summary>
    public string assetName;

    /// <summary>
    /// project name associated wit this scene bundle
    /// </summary>
    public string projectName;

    /// <summary>
    /// List of scenes to load
    /// </summary>
    public List<SceneBundle> scenes = new List<SceneBundle>();
}
