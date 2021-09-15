using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Load all the scenes that the project has in the build
    void Start()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            if(i != 0)
            SceneManager.LoadSceneAsync(i, LoadSceneMode.Additive);
        }
    }
}
