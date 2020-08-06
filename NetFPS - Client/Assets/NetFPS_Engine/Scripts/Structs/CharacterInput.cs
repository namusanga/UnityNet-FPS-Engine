using UnityEngine;

public struct CharacterInput
{

    //the input we have made
    public Vector2 dir;//the direction of the input
    public int inputNum;//the number of this input
    public Vector3 predictedResult;//predicted result if any
    public Quaternion rotation;//rotation of character when this input was made


    public CharacterInput(Vector2 _dir, int _inputNum, Vector3 _predictedPosition, Quaternion _rotation)
    {
        dir = _dir;
        inputNum = _inputNum;
        predictedResult = _predictedPosition;
        rotation = _rotation;
    }

    /// <summary>
    /// write this input to a packet
    /// </summary>
    /// <param name="_packet"></param>
    public void WriteToPacket(Packet _packet)
    {
        _packet.Write(dir.x);
        _packet.Write(dir.y);
        _packet.Write(inputNum);
        _packet.Write(predictedResult);
        _packet.Write(rotation);
    }


    /// <summary>
    /// read this input from a packet
    /// </summary>
    /// <param name="_packet"></param>
    public void ReadFromPacket(Packet _packet)
    {
        dir = new Vector2(_packet.ReadFloat(), _packet.ReadFloat());
        inputNum = _packet.ReadInt();
        predictedResult = _packet.ReadVector3();
        rotation = _packet.ReadQuaternion();
    }
}
