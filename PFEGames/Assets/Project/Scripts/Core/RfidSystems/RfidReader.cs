using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

public class RfidReader : MonoBehaviour
{
    [Header("Port série")]
    public string portName = "COM9";
    public int baudRate = 115200;

    [Header("Anti-répétition")]
    [Tooltip("Temps sans lecture avant qu'un même tag puisse re-déclencher")]
    public float releaseDelay = 1.2f;

    public event Action<string> OnTagRead;

    SerialPort port;
    Thread thread;
    volatile bool running;
    volatile string pendingUid;

    string currentUid;
    float lastSeenTime;

    static readonly Regex uidRegex =
        new Regex("^[0-9A-Fa-f]{8}([0-9A-Fa-f]{6})?$");

    void Start()
    {
        port = new SerialPort(portName, baudRate) { ReadTimeout = 100 };
        try
        {
            port.Open();
        }
        catch (Exception e)
        {
            Debug.LogError("Impossible d'ouvrir " + portName + " : " + e.Message);
            return;
        }

        running = true;
        thread = new Thread(ReadLoop);
        thread.Start();
    }

    void ReadLoop()
    {
        while (running)
        {
            try
            {
                string line = port.ReadLine().Trim();
                if (uidRegex.IsMatch(line))
                    pendingUid = line.ToUpper();
            }
            catch (TimeoutException) { }
            catch (Exception) { if (!running) break; }
        }
    }

    void Update()
    {
        string uid = pendingUid;
        pendingUid = null;

        if (uid != null)
        {
            bool sameTagStillThere =
                uid == currentUid && Time.time - lastSeenTime < releaseDelay;

            lastSeenTime = Time.time;

            if (!sameTagStillThere)
            {
                currentUid = uid;
                Debug.Log("Tag lu : " + uid);
                OnTagRead?.Invoke(uid);
            }
        }
    }

    void OnDestroy()
    {
        running = false;
        thread?.Join();
        if (port != null && port.IsOpen) port.Close();
    }
}