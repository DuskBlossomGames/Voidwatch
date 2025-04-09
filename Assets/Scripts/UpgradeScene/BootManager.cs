using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using static Static_Info.PlayerData;

public class BootManager : MonoBehaviour
{
    public string onPlayDest;
    public string tutorialDest;
    void Start()
    {
        PlayerDataInstance.Health = PlayerDataInstance.maxHealth;
        if (PlayerDataInstance.gotoTitle)
        {
            PlayerDataInstance.gotoTitle = false;
            SceneManager.LoadScene("TitleScreen");
        } else if (PlayerDataInstance.isInTutorial)
        {
            SceneManager.LoadScene(tutorialDest);
        } else
        {
            SceneManager.LoadScene(onPlayDest);
        }
        
    }
}


public class SerializedDict<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    public List<TKey> _keys;
    public List<TValue> _values;
    
    public void OnBeforeSerialize()
    {
        Debug.Log("Serializing");
        _keys.Clear();
        _values.Clear();

        foreach (var key in base.Keys)
        {
            _keys.Add(key);
            _values.Add(base[key]);
        }
    }
    public void OnAfterDeserialize()
    {
        Debug.Log("Deserializing");
        base.Clear();
        for (int i = 0; i != Mathf.Min(_keys.Count, _values.Count); i++)
            base.Add(_keys[i], _values[i]);
    }

    void OnGUI()
    {
        foreach (var key in base.Keys)
            GUILayout.Label("Key: " + key + " value: " + base[key]);
    }

}
