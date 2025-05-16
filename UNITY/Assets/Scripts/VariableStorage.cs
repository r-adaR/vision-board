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

    
    public T TryGet<T>(string key)
    {
        System.Object o;
        variables.TryGetValue(key, out o);

        if (o != null && o is T)
        {
            return (T)o;
        }

        else
        {
            Debug.LogError("variable "+key+" is not stored as a "+typeof(T).FullName);
            return default;
        }
    }
    
}
