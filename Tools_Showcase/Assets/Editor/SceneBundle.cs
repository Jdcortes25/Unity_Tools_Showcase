using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SceneBundle
{
    /// <summary>
    /// File path of the scene to use
    /// </summary>
    public string scenePath;

    /// <summary>
    /// Should this scene be loaded or used when making a build
    /// </summary>
    public bool isSceneEnabled;

    /// <summary>
    /// Initialize a new instance of SceneBundle with all variables assigned
    /// </summary>
    /// <param name="path"></param>
    /// <param name="enabled"></param>
    public SceneBundle(string path, bool enabled)
    {
        scenePath = path;
        isSceneEnabled = enabled;
    }
}
