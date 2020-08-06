using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LatencyMeasurer : MonoBehaviour
{
    public Gradient valueGradient;
    [Range(200, 500)]
    public int badLatency = 300;
    
    Text textValue;

    void Start()
    {
        textValue = GetComponent<Text>();
    }

    void Update()
    {
        var ping = -1;
        if (UnityEngine.Networking.NetworkManager.singleton.client != null)
            ping = UnityEngine.Networking.NetworkManager.singleton.client.GetRTT();
        if (ping >= 0)
        {
            textValue.text = ping.ToString();
            textValue.color = valueGradient.Evaluate(ping / badLatency);
        }
        else
        {
            textValue.text = "Off";
            textValue.color = valueGradient.Evaluate(1);
        }
    }
}