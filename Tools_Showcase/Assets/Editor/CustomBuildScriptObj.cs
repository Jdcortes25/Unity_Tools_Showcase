using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CustomBuildScriptObj : ScriptableObject
{
    /// <summary>
    /// name of the customBuild
    /// </summary>
    public string assetName;

    /// <summary>
    /// File path for where the build should be done
    /// </summary>
    public string filePath;

    /// <summary>
    /// List of scene bundles
    /// </summary>
    public List<SceneBundleScriptObj> sceneBundleList = new List<SceneBundleScriptObj>();
}
