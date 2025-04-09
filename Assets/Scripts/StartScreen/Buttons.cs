using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Static_Info.PlayerData;

public class Buttons : MonoBehaviour
{
    private void Start()
    {
        PlayerDataInstance.Health = PlayerDataInstance.maxHealth;
    }

    public void Play()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void Tutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void Options()
    {
        throw new Exception("Unimplemented");
    }

    public void Credits()
    {
        throw new Exception("Unimplemented");
    }

    public void Quit()
    {
        UnityEngine.Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

}
