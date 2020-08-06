using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class AuthoritativeCharacter : MonoBehaviour
{
    public bool isLocalPlayer = false;  

    public float Speed { get { return speed; } }
    public float LookSpeed { get { return lookSpeed; } }
    /// <summary>
    /// Controls how many inputs are needed before sending update command
    /// </summary>
    public int InputBufferSize { get; private set; }

    /// <summary>
    /// Controls how many input updates are sent per second
    /// </summary>
    [SerializeField, Range(10, 50), Tooltip("In steps per second")]
    int inputUpdateRate = 20;
    [HideInInspector, SerializeField, Range(5f, 15f)]
    float speed = 15f;
    [HideInInspector]
    float lookSpeed = 2.0f;
    [SerializeField, Range(1, 60), Tooltip("In steps per second")]
    public int interpolationDelay = 12;//time from one interpolcation to the next


    public static List<AuthoritativeCharacter> observers = new List<AuthoritativeCharacter>();
    /// <summary>
    /// shared state for this character across the entire network
    /// </summary>
    public CharacterState state
    {
        get
        {
            return _state;
        }
        set
        {
            _state = value;
            OnServerUpdate(value);
        }
    }
    private CharacterState _state;
    
    IAuthCharStateHandler stateHandler;

    private CharacterController charCtrl;

    private PlayerInfo player
    {
        get
        {
            return GetComponent<PlayerInfo>();
        }
    }

    void Awake()
    {
        //find how manay inputs we should hold before sending them and refreshing
        //first gets the number of inputs that will be got per second
        //then divides that by the number of inputs we should send every second
        //this determines how bug our holder for input should be before we send the input out
        InputBufferSize = (int)(1 / Time.fixedDeltaTime) / inputUpdateRate;


        //get a callback when the server state of the player changes
        if (Client.packetHandlers.ContainsKey((int)ServerPackets.BroadcastPlayerState) == false)
        {
            Client.packetHandlers.Add((int)ServerPackets.BroadcastPlayerState, HandlePlayerServerUpdate);
        }

        //get the position of the player and make it the active position
        state = new CharacterState
        {
            position = transform.position,
            rotation = transform.rotation,
            moveNum = state.moveNum,
            timestamp = state.timestamp
        };
    }


    void Start()
    {
        //REMOTE PLAYER ONLY
        if (!isLocalPlayer)
        {
            stateHandler = gameObject.GetComponent<AuthCharObserver>();//used to observer the state of this player on the server
            return;
        }

        //LOCAL PLAYER ONLY
        GetComponentInChildren<Renderer>().material.color = Color.green;//make our character green
        stateHandler = gameObject.GetComponent<AuthCharPredictor>();//used to predict the state of our local player
        gameObject.GetComponent<AuthCharInput>();//used to make local input and transmit it to server
    }

    private void OnEnable()
    {
        //get the character controller of this player
        charCtrl = GetComponent<CharacterController>();

        //add this as an observer
        observers.Add(this);
    }

    private void OnDisable()
    {
        observers.Remove(this);
    }

    /// <summary>
    /// move this character to a give location, from a calculated state
    /// </summary>
    /// <param name="overrideState"></param>
    public void SyncPosition(CharacterState overrideState)
    {
        charCtrl.Move(overrideState.position - transform.position);//save the position
    }

    public void SyncRotation(CharacterState overrideState)
    {
        transform.rotation = overrideState.rotation;//save the rotation
    }

    /// <summary>
    /// called on the server after a state update
    /// called on clients when the server state changes
    /// </summary>
    /// <param name="newState"></param>
    public void OnServerUpdate(CharacterState newState)
    {
        //use the server observer to sync to the server state
        if (stateHandler != null)
            stateHandler.OnServerUpdate(state);
    }

    public static void HandlePlayerServerUpdate(Packet _packet)
    {
        int _playerID = _packet.ReadInt();
        for (int i = 0; i < observers.Count; i++)
        {
            if (observers[i].player.id == _playerID)
            {
                CharacterState newState = new CharacterState();
                newState.Readpacket(_packet);
                //save the new state
                observers[i].state = newState;

                //if (observers[i].isLocalPlayer)
                //    Logger.Logger.Log(Logger.LogModeOptions.Verbose, $"Server Override -- Timestamp :: {newState.moveNum}");
                return;
            }
        }
    }

    //send the inputs to the server
    public void ServerMove(CharacterInput[] inputs)
    {
        //Logger.Logger.Log(Logger.LogModeOptions.Verbose,$"SENDING INPUTS --  Size :: { inputs.Length} -- Timestamp :: {inputs[0].inputNum}");

        //send the inputs to the server
        using (Packet _packet = new Packet((int)ClientPackets.CmdMove))
        {
            _packet.Write(inputs.Length);//length of packets
            //write all the inputs
            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i].WriteToPacket(_packet);
            }
            //send the data
            ClientSend.SendTCPData(_packet);
        }
    }
}