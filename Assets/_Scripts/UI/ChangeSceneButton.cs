using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSceneButton : MonoBehaviour
{
    public void ChangeScene(int sceneNum)
    {
        SceneManagementSystem.Instance.LoadScene(sceneNum);
    }
}
