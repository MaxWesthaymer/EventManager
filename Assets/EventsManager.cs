using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using Analytics;
using UnityEngine.Networking;
using EventType = Analytics.EventType;

public class EventsManager : MonoBehaviour
{
    #region Propierties
    public static EventsManager Instance { get; private set; }
    public Data Data { get; private set; }

    #endregion
    #region UnityMethods
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        LoadData();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
           TrackEvent(EventType.LEVEL_START, new object[] {1});
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            TrackEvent(EventType.LEVEL_WIN, new object[] {1});
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            TrackEvent(EventType.LEVEL_LOSE, new object[] {1});
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            TrackEvent(EventType.INGAME_PURCHASE, new object[] {"bo", 500});
        }
    }
    
    #endregion
    #region PublicMethods

    public static void TrackEvent(EventType type, object[] data = null)
    {
        var key = type.ToString().ToLower();
        var parameters = new Dictionary<string, object>();
        
        parameters.Add("time", DateTime.Now);
        parameters.Add("version", Application.version);

        switch (type)
        {
            case EventType.LEVEL_START :
            {
                parameters.Add("level" , data[0]);
                break;
            }
            case EventType.LEVEL_WIN :
            {
                parameters.Add("level" , data[0]);
                break;
            }
            case EventType.LEVEL_LOSE :
            {
                parameters.Add("level" , data[0]);
                break;
            }
            case EventType.INGAME_PURCHASE :
            {
                parameters.Add("product" , data[0]);
                parameters.Add("total_cost" , data[1]);
                break;
            }
            default:
                break;
        }
        Instance.SendEvent(new Event(key, parameters));
    }

    private void SendEvent(Event value)
    {
        Data.events.Add(value);
        Save();
    }
    public IEnumerator CallLogin(string url, string data)
    {
        var request = new UnityWebRequest (url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler =  new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.error != null)
        {
            Debug.Log("Error: " + request.error);
        }
        else
        {
            Debug.Log("All OK");
            Debug.Log("Status Code: " + request.responseCode);
        }

    }

    public void Save()
    {
        SaveData(Data);
    }
    #endregion
    
    #region PrivateMethods
    private void SaveData(object obj)
    {
        var str = JsonConvert.SerializeObject(obj);
        Debug.Log(str);
        PlayerPrefs.SetString("savingdata", str);
    }
    
    private void LoadData()
    {
        var str = PlayerPrefs.GetString("savingdata");
        if (str == String.Empty)
        {
            Data = new Data();
            Data.events = new List<Event>();
        }
        else
        {
            Data = JsonConvert.DeserializeObject<Data>(str);
        }
        Save();
    }

    private void OnApplicationQuit()
    {
        Save();
    }
    #endregion

}
[Serializable]
public class Data
{
    public List<Event> events;
}

[Serializable]
public class Event
{
    public Event(string typeName, Dictionary<string, object> parameters)
    {
        type = typeName;
        data = parameters;
    }
    public string type;
    public Dictionary<string, object> data;
}




