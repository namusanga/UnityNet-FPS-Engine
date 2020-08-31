using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AuthoritativeCharacter : MonoBehaviour
{
    public float Speed { get { return speed; } }
    /// <summary>
    /// Controls how many inputs are needed before sending update command
    /// </summary>
    public int InputBufferSize { get; private set; }

    /// <summary>
    /// Controls how many input updates are sent per second
    /// </summary>
    [SerializeField, Range(10, 50), Tooltip("In steps per second")]
    int inputUpdateRate = 10;
    [HideInInspector, SerializeField, Range(5f, 15f)]
    float speed = 15f;
    [SerializeField, Range(1, 60), Tooltip("In steps per second")]
    public int interpolationDelay = 12;

    public static List<AuthoritativeCharacter> serverPlayers = new List<AuthoritativeCharacter>();
    public Player myPlayer;

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
        }
    }

    [HideInInspector] public ServerCharacterShooter shooter;

    private CharacterState _state;

    IAuthCharStateHandler stateHandler;
    public ServerCharacter server;

    CharacterController charCtrl;

    void Awake()
    {
        //find how manay inputs we should hold before sending them and refreshing
        //first gets the number of inputs that will be got per second
        //then divides that by the number of inputs we should send every second
        //this determines how bug our holder for input should be before we send the input out
        InputBufferSize = (int)(1 / Time.fixedDeltaTime) / inputUpdateRate;
        OnStartServer();
        charCtrl = GetComponent<CharacterController>();//get the character controller of this player
    }


    //SERVER ONLY
    public void OnStartServer()
    {
        server = gameObject.GetComponent<ServerCharacter>();

        //register for call backs to move player on server
        if (Server.packetHandlers.ContainsKey((int)ClientPackets.CmdMove) == false)
        {
            //add the handler
            Server.packetHandlers.Add((int)ClientPackets.CmdMove, CmdMoveStatic);
        }
    }

    private void OnEnable()
    {
        serverPlayers.Add(this);
        myPlayer = GetComponent<Player>();
    }

    private void OnDisable()
    {
        serverPlayers.Remove(this);
    }



    /// <summary>
    /// move this character to a give location, from a calculated state
    /// </summary>
    /// <param name="overrideState"></param>
    public void SyncState(CharacterState overrideState)
    {
        charCtrl.Move(overrideState.position - transform.position);
    }

    public static void CmdMoveStatic(int _fromClient, Packet _packet)
    {
        //check for the player that is being told to mvoe
        foreach (AuthoritativeCharacter authoritativeCharacterServer in serverPlayers)
        {
            if (authoritativeCharacterServer.myPlayer.id == _fromClient)
            {
                CharacterInput[] _inputs = new CharacterInput[_packet.ReadInt()];
                
                for (int i = 0; i < _inputs.Length; i++)
                {
                    _inputs[i] = new CharacterInput();
                    _inputs[i].ReadFromPacket(_packet);
                }

                authoritativeCharacterServer.CmdMove(_inputs);
            }
        }
    }

    public void CmdMove(CharacterInput[] inputs)
    {
        server.Move(inputs);
    }
}