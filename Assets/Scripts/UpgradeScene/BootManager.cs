using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;

public class BootManager : MonoBehaviour
{
    public MerchantData merchantData;
    public Scriptable_Objects.PlayerData playerData;
    public BulletInfo def;
    public BulletInfo curr;
    void Start()
    {
        foreach (FieldInfo fi in def.GetType().GetFields())
        {
            fi.SetValue(curr, fi.GetValue(def));
        }

        playerData.Scrap = 0;
        merchantData.currentShopID = 0;
        merchantData.Shops = new SerializedDict<uint, MerchantData.MerchantObj>();
    }

    private void LateUpdate()
    {
        SceneManager.LoadScene("LevelSelect");
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
