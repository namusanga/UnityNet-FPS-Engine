using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Linq;

public class ServerStarter : MonoBehaviour
{
    void Start()
    {
        var args = Environment.GetCommandLineArgs();
        if (args.Any((arg) => arg == "servermode"))
        {
            UnityEngine.Networking.NetworkManager.singleton.StartServer();
        }
        else if (args.Any((arg) => arg == "gamemode"))
        {
            UnityEngine.Networking.NetworkManager.singleton.StartClient();
            //AuthCharInput.simulated = true;
        }
        Time.fixedDeltaTime = 1 / 60f;
        Destroy(gameObject);
    }
}