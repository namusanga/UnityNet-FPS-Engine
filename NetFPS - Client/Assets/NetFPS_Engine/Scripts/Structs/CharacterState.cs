using UnityEngine;


/// <summary>
/// holds the state of the player including position,rotation,velocity etc
/// </summary>
[System.Serializable]
public struct CharacterState
{
    public Vector3 position;
    public Quaternion rotation;
    public int moveNum;
    public int timestamp;

    //get info about this state
    public override string ToString()
    {
        return string.Format("CharacterState Pos:{0}|Rot:{1}|Vel:{2}|AngVel:{3}|MoveNum:{4}|Timestamp:{5}", position, rotation, moveNum, timestamp);
    }


    //returns a state for a new character before anything is done to him
    public static CharacterState Zero
    {
        get
        {
            return new CharacterState
            {
                position = Vector3.zero,
                rotation = Quaternion.identity,
                moveNum = 0,
                timestamp = 0
            };
        }
    }

    /// <summary>
    /// get the interpolcated state of the from char to the to char given a selected time
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="clientTick"></param>
    /// <returns></returns>
    public static CharacterState Interpolate(CharacterState from, CharacterState to, int clientTick)
    {
        //the time to use in the interpolation
        float t = ((float)(clientTick - from.timestamp)) / (to.timestamp - from.timestamp);

        return new CharacterState
        {
            position = Vector3.Slerp(from.position, to.position, t),
            rotation = Quaternion.Slerp(from.rotation, to.rotation, t),
            moveNum = 0,
            timestamp = 0
        };
    }


    public static CharacterState Move(CharacterState previous, CharacterInput input, float speed, int timestamp, Transform _transformToMove)
    {
        //update transform rotation
        _transformToMove.rotation = input.rotation;

        //calculate movement vector
        //horizontal movement
        Vector3 movement = GetMovement(_transformToMove, input, speed);


        //calculate the next movement
        CharacterState state;

        state = new CharacterState
        {
            position = movement + previous.position,
            rotation = input.rotation,
            moveNum = previous.moveNum + 1,
            timestamp = timestamp
        };

        return state;
    }

    public static Vector3 GetMovement(Transform _transformToMove, CharacterInput _input, float _speed)
    {
        float _gravityPull = GameSettings.Instance.gravity * Time.fixedDeltaTime;

        //horizontal movement
        Vector3 _movement = (_transformToMove.forward * _input.dir.y)
                          //vertical movement
                          + (_transformToMove.right * _input.dir.x)
                          + (_gravityPull * Vector3.up);

        _movement *= (_speed * Time.fixedDeltaTime);

        return _movement;
    }
    public static bool ValidatePredictedState(Vector3 _prediction, Vector3 _calculatedPosition)
    {
        if (Vector3.Distance(_prediction, _calculatedPosition) < 5)
            return true;
        else
            return false;
    }

    public void WritePacket(Packet _packet)
    {
        _packet.Write(position);
        NetworkOptimization.WriteCompressedRotation(_packet, rotation);
        _packet.Write(moveNum);
        _packet.Write(timestamp);
    }

    public void Readpacket(Packet _packet, System.Action onFinished = null)
    {
        position = _packet.ReadVector3();
        rotation = NetworkOptimization.ReadCompressedRotation(_packet);
        moveNum = _packet.ReadInt();
        timestamp = _packet.ReadInt();

        //run any functionality for after the packet has been read
        onFinished?.Invoke();
    }
}