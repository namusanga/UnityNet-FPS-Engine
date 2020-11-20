using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class NetSharedObjectSpawner : MonoBehaviour, SceneInitComponent
{

    //SUB PACKET INT FOR NET SHARED OBJECT SPAWNER
    public int subPacketInt = 100;
    //ALL SHARED PREFABS
    public List<NetSharedObject> sharedPrefabs = new List<NetSharedObject>();
    //ALL SHARED OBJECTS (SPAWNED OBJECTS)
    [HideInInspector] public List<NetSharedObject> sharedObjects = new List<NetSharedObject>();

    //SINGLETON
    public static NetSharedObjectSpawner Active
    {
        get
        {
            if (_active == null)
                _active = FindObjectOfType<NetSharedObjectSpawner>();
            return _active;
        }
    }
    private static NetSharedObjectSpawner _active;
    
    private void OnEnable()
    {
        InitScenePacker.Active.sceneInitComponents.Add(this);
    }

    
    /// <summary>
    /// let us know a new obejct has been created so we can update the other clients and it to the active gameobjects
    /// </summary>
    public void ObjectEnabled(NetSharedObject _netSharedObject)
    {
        sharedObjects.Add(_netSharedObject);
    }

    /// <summary>
    /// packet to be sent over the network
    /// </summary>
    public SubPacket GetSubPacket()
    {
        SubPacket _subPacket = new SubPacket();
        //<==OBJECT COUNT
        _subPacket.Write(sharedObjects.Count);
        //CHECK FOR ALL NET SHARED OBJECTS
        for (int i = 0; i < sharedObjects.Count; i++)
        {
            //pack the:
            //<==GUID
            _subPacket.Write(sharedObjects[i].guid.ToString());
            //<==NAME
            _subPacket.Write(sharedObjects[i].gameObject.name);
            //<==POSITION
            _subPacket.Write(sharedObjects[i].transform.position);
            //<==ROTATION
            _subPacket.Write(sharedObjects[i].transform.rotation);
            //<==SCALE    
            _subPacket.Write(sharedObjects[i].transform.localScale);
        }
        
        return _subPacket;
    }
    
    public int SubPacketID()
    {
        return subPacketInt;
    }

    /// <summary>
    /// read the packet we get from over the network
    /// </summary>
    public void HandleSubPacket(Packet _packet)
    {
        SubPacket _subPacket = new SubPacket();
        _subPacket.Read(_packet);
        
        //==> OBJECT COUNT
        int _count = _subPacket.ReadInt();
        for (int i = 0; i < _count; i++)
        {
            //==> GUID
            string _guid = _subPacket.ReadString();
            //==> NAME
            string _name = _subPacket.ReadString();
            //==> POS
            Vector3 _pos = _subPacket.ReadVector3();
            //==> ROT
            Quaternion _rot = _subPacket.ReadQuaternion();
            //==> SCALE
            Vector3 _scale = _subPacket.ReadVector3();

            //SPAWN THE OBJECT
            SpawnNetObject(_guid,_name, _pos, _rot,_scale);

        }
    }

    //spawn a network object using the guid 
    private void SpawnNetObject(string _guid,string _name, Vector3 _pos, Quaternion _rot, Vector3 _scale)
    {
        GameObject _spawn =
            Instantiate(sharedPrefabs.Find(x => x.guid.ToString() == _guid).gameObject, _pos, _rot, null);
        _spawn.name = _name;
    }
}
