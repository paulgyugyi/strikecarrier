using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Helper scipt for buttons that simply load a scene.
public class LoadScene : MonoBehaviour
{
    // Number of the scene to load.
    public int scene = 0;

    public void Load()
    {
        SceneManager.LoadScene(scene);
    }
}
