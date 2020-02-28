using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private void Awake()
    {
        // TODO: Move to general ApplicationController
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public void SwitchScene(int index)
    {
        SceneManager.LoadScene(index, LoadSceneMode.Single);
    }
}
