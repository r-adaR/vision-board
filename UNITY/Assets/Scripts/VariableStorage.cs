using System.Collections.Generic;
using UnityEngine;

public class VariableStorage : MonoBehaviour
{
    public static VariableStorage instance;

    private Dictionary<string, System.Object> variables = new Dictionary<string, System.Object>();

    void Awake()
    {
        if (instance != null) Destroy(gameObject);
        
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Put(string key, System.Object value)
    {
        variables[key] = value;
    }


    /// <summary>
    /// gets the object assigned to string "key." returns defaultValue if no object found at "key."
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public T TryGet<T>(string key, T defaultValue = default)
    {
        System.Object o;
        if (!variables.TryGetValue(key, out o))
        {
            return defaultValue;
        }

        if (o is T)
        {
            return (T)o;
        }
        else
        {
            Debug.LogError("variable " + key + " is not stored as a " + typeof(T).FullName);
            return default;
        }
    }

}
