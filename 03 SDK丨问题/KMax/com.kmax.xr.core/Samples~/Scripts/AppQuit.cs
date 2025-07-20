using KmaxXR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AppQuit : MonoBehaviour
{
    [SerializeField]
    Text title;
    void Start()
    {
        if (title != null)
        {
            title.text += $"\nSDK Version: {KmaxNative.SDKVersion}\n{KmaxNative.DeviceId}";
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.buildIndex != 0)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                Application.Quit();
            }
        }
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }

    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }
}
