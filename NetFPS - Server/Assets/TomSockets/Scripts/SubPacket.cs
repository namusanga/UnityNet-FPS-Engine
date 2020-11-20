
using System.Collections;
using System.Collections.Generic;

public class SubPacket:Packet
{

    public SubPacket()
    {
        buffer = new List<byte>(); // Initialize buffer
        readPos = 0; // Set readPos to 0
    }

    public void Read(Packet _packet)
    {
        //read the length of the packet
        int _length = _packet.ReadInt();
        //init the buffer
        buffer= new List<byte>();
        //read all the data for the given length
        buffer.AddRange(_packet.ReadBytes(_length));
        readableBuffer = buffer.ToArray();
        readPos = 0;
    }

    public void Write(Packet _packet)
    {
        //<==Length
        _packet.Write(buffer.Count);
        //<==Data
        _packet.Write(buffer.ToArray());
    }
}