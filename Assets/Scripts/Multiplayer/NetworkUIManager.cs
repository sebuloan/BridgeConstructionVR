
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUIManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject selectionPanel;
    [SerializeField]
    private TextMeshProUGUI selectionStatusText;
    [SerializeField]
    private TMP_InputField ipInputField;

    [SerializeField]
    private TextMeshProUGUI connectionStatusText;

    [SerializeField]
    private GameObject connectionPanel;
    /*  [SerializeField]
      private TextMeshProUGUI resultText;*/
    /* [SerializeField]
     private GameObject resultPanel;*/
    [SerializeField]
    private GameObject nameInputPanel;
    [SerializeField]
    private TMP_InputField nameInputField;
    [SerializeField]
    private TextMeshProUGUI nameStatusText;

    private ushort _selectedPort = 7777;
    /*  private NetworkVariable<bool> isCorrect = new NetworkVariable<bool>(
         false,
         NetworkVariableReadPermission.Everyone,
         NetworkVariableWritePermission.Server
     );*/

    private void Start()
    {
        selectionStatusText.text = "Drill Inspector or Recruit";
        selectionPanel.SetActive(true);
    }

    public void StartHost()
    {
        var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        transport.Shutdown();

        _selectedPort = 7777;
        Debug.Log("Selected port is " + _selectedPort);

        SetHostConnectionData(_selectedPort);

        if (NetworkManager.Singleton.StartHost())
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            selectionStatusText.text = $"Hosting on port {_selectedPort}\nIP: {GetLocalIPAddress()}";
            selectionPanel.SetActive(false);
            nameInputPanel.SetActive(true);
        }
        else
        {
            selectionStatusText.text = "Recruit joining failed!";
        }
    }

    private void SetHostConnectionData(int port)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        string hostAddress = "0.0.0.0";
        transport.ConnectionData.Address = hostAddress;
        transport.ConnectionData.Port = (ushort)port;
    }

    private string GetLocalIPAddress()
    {
        string ipAddress = "127.0.0.1";
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddress = ip.ToString();
                    break;
                }
            }
        }
        catch { }
        return ipAddress;
    }

    //Trainer
    public void StartClient()
    {
        selectionPanel.SetActive(false);
        connectionPanel.SetActive(true);
        connectionStatusText.text = "Enter IP to Connect";
    }

    public void OnConnectClientClicked()
    {
        SetConnectionData();

        if (NetworkManager.Singleton.StartClient())
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            connectionStatusText.text = "Connecting to " + ipInputField.text + ":" + _selectedPort;
            StartCoroutine(CheckClientConnectionTimeout(5f));
        }
        else
        {
            connectionStatusText.text = "Failed to start Drill Instructor!";
        }
    }

    private void SetConnectionData()
    {
        if (string.IsNullOrEmpty(ipInputField.text))
        {
            Debug.Log("IP is empty");
            connectionStatusText.text = "Please enter the IP address";
            return;
        }
        else
        {
            string ip = ipInputField.text.Trim();
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.ConnectionData.Address = ip;
            transport.ConnectionData.Port = (ushort)_selectedPort;
            Debug.Log("IP and Port set to: " + ip + ":" + _selectedPort);
        }
    }
    private IEnumerator CheckClientConnectionTimeout(float timeout)
    {
        float timer = 0f;
        while (!NetworkManager.Singleton.IsConnectedClient && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        if (!NetworkManager.Singleton.IsConnectedClient)
        {
            connectionStatusText.text = "Unable to connect Recruit. Please check IP";
        }
    }

    private void OnDisable()
    {
        // isCorrect.OnValueChanged -= OnResultChanged;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            connectionPanel.SetActive(false);
            connectionStatusText.text = $"Trainer connected: {clientId}";
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}");
        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            connectionStatusText.text = $"Drill Instructor disconnected: {clientId}";
        }
    }

    public void OnNameEntered()
    {
        if (string.IsNullOrEmpty(nameInputField.text))
        {
            Debug.Log("Recruit name is empty");
            nameStatusText.text = "Please enter the Recruit name";
            return;
        }
        else
        {
            string traineeName = nameInputField.text.Trim();
            PlayerPrefs.SetString("RecruitName", traineeName);
            Debug.Log("Recruit name is " + traineeName);
            nameInputPanel.SetActive(false);
            // resultPanel.SetActive(true);
        }
    }
}
