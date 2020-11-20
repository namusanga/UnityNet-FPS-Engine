using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

        Server.Start(50, 26951);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public NetPlayer InstantiatePlayer()
    {
        NetPlayer _player = Instantiate(playerPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity).GetComponent<NetPlayer>();
        return _player;

    }
}
