using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using EventType = Analytics.EventType;

public class EventsManager : MonoBehaviour
{
    #region InspectorFields
    [SerializeField] private string serverURL  = "url";
    [SerializeField] private float cooldownBeforeSend  = 1f;
    #endregion
    #region Propierties
    public static EventsManager Instance { get; private set; }
    private Data Data;
    private float currentCooldownTime;
    private List<Event> sendingsEvents = new List<Event>();

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

    private void Start()
    {
        if (Data.events.Count > 0)
        {
            TryToSendEvents();
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
    
    #endregion
    
    #region PrivateMethods
    private void SendEvent(Event value)
    {
        Data.events.Add(value);
        SaveData(Data);
        TryToSendEvents();
    }

    private void TryToSendEvents()
    {
        if (currentCooldownTime <= 0)
        {
            currentCooldownTime = cooldownBeforeSend;
            StartCoroutine(WaitToSend());
        }
    }

    private IEnumerator WaitToSend()
    {
        while (currentCooldownTime > 0)
        {
            currentCooldownTime -= Time.deltaTime;
            yield return null;
        }
        
        var data = JsonConvert.SerializeObject(Data);
        sendingsEvents.Clear();
        sendingsEvents.AddRange(Data.events);
        Data.events.Clear();
        StartCoroutine(SendEvent(serverURL, data));
    }
    private IEnumerator SendEvent(string url, string data)
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
            Data.events.InsertRange(0, sendingsEvents);
            sendingsEvents.Clear();
            SaveData(Data);
            TryToSendEvents();
        }
        else
        {
            Debug.Log("All OK");
            Debug.Log("Status Code: " + request.responseCode);
            sendingsEvents.Clear();
        }
    }
    

    private void SaveData(object obj)
    {
        var str = JsonConvert.SerializeObject(obj);
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
        SaveData(Data);
    }

    private void OnApplicationQuit()
    {
        if (sendingsEvents.Count > 0)
        {
            Data.events.InsertRange(0, sendingsEvents);
        }
        SaveData(Data);
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