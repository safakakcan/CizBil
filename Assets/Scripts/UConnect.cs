using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using UnityEngine;

public class UConnect : MonoBehaviour
{
    public bool Connected { get { return client.Connected; } }

    [Header("Host Address")]
    [SerializeField]
    private string host = "127.0.0.1";
    [SerializeField]
    private int port = 8090;
    [SerializeField]
    private bool autoConnect = true;
    [SerializeField]
    private bool runInterScenes = false;

    TcpClient client = new TcpClient();
    NetworkStream stream;
    StreamReader reader;
    StreamWriter writer;
    System.Threading.Thread thread;
    List<string> buffer = new List<string>();

    [Header("Network Event Filters")]
    public List<EventFilter> eventFilters = new List<EventFilter>();

    // Start is called before the first frame update
    void Start()
    {
        if (runInterScenes)
            DontDestroyOnLoad(this.gameObject);

        if (autoConnect && !Connected)
            Connect();
    }

    // Update is called once per frame
    void Update()
    {
        if (buffer.Count > 0)
        {
            try
            {
                NetworkData data = JsonUtility.FromJson<NetworkData>(buffer[0]);

                foreach (EventFilter filter in eventFilters)
                {
                    if (filter.Compare(data.cmd))
                    {
                        filter.onMatched.Invoke(data);
                        break;
                    }
                }
            }
            catch { }

            buffer.RemoveAt(0);
        }
    }

    public void Connect()
    {
        client = new TcpClient(host, port);
        stream = client.GetStream();
        reader = new StreamReader(stream);
        writer = new StreamWriter(stream);
        thread = new System.Threading.Thread(() => Listen());
        thread.Start();
    }

    public void Disconnect()
    {
        client.Close();
        thread.Interrupt();
    }

    void Listen()
    {
        while (client.Connected)
        {
            string json = reader.ReadLine();
            buffer.Add(json);
        }
    }

    void Send(Request request)
    {
        if (Connected && writer != null)
        {
            string json = JsonUtility.ToJson(request);
            writer.WriteLine(json);
            writer.Flush();
        }
    }

    public void SendRequest(params string[] parameters)
    {
        Request request = new Request();
        request.cmd = parameters;
        request.await = true;
        Send(request);
    }

    public void CallEvent(params string[] parameters)
    {
        Request request = new Request();
        request.cmd = parameters;
        request.await = false;
        Send(request);
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}

[System.Serializable]
public class EventFilter
{
    public string schema;
    public UnityEngine.Events.UnityEvent<NetworkData> onMatched;

    public EventFilter(string Schema, UnityEngine.Events.UnityEvent<NetworkData> OnMatchedEvent = null)
    {
        schema = Schema;
        onMatched = OnMatchedEvent;
    }

    public bool Compare(string input)
    {
        try
        {
            string[] filters = schema.Split('*');

            for (int i = 0; i < filters.Length; i++)
            {
                if (i == filters.Length - 1)
                {
                    if (filters[i] == "")
                    {
                        continue;
                    }
                    else
                    {
                        if (filters[i] == input)
                        {
                            continue;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else if (i == 0)
                {
                    if (filters[i] == "")
                    {
                        int start = input.IndexOf(filters[i + 1]);
                        if (start != -1)
                        {
                            input = input.Substring(start);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        int start = input.IndexOf(filters[i + 1]);
                        if (start != -1 && filters[i] == input.Substring(0, filters[i].Length))
                        {
                            input = input.Substring(start);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    int start = input.IndexOf(filters[i + 1]);
                    if (start != -1 && filters[i] == input.Substring(0, filters[i].Length))
                    {
                        input = input.Substring(start);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}

[System.Serializable]
public class NetworkData
{
    public string cmd;
    public string[] array;
    public string single { get { return (array != null && array.Length > 0) ? array[0] : null; } }

    public NetworkData(params string[] _array)
    {
        array = _array;
    }
}

[System.Serializable]
public class DataCell
{
    public string id = "";
    public string value = "";
    public bool file = false;
    public Permission permission = new Permission();
    public List<DataCell> children = new List<DataCell>();
}

[System.Serializable]
public class Permission
{
    public Access read = Access.Private;
    public Access write = Access.Private;
}

[System.Serializable]
public enum Access
{
    Public,
    Private,
    Authority
}

[System.Serializable]
public class Request
{
    public string[] cmd;
    public bool await;
}