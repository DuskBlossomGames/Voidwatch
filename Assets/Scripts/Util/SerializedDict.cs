using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public class SerializedDict<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        public List<TKey> _keys;
        public List<TValue> _values;
    
        public void OnBeforeSerialize()
        {
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
            base.Clear();
            for (int i = 0; i != Mathf.Min(_keys.Count, _values.Count); i++)
                base.Add(_keys[i], _values[i]);
        }
    }

}