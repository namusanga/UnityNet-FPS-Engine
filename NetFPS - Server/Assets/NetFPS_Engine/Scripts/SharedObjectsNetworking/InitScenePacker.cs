using System;
using System.Collections.Generic;
using Devdog.InventoryPro;
using UnityEngine;

public class InitScenePacker : MonoBehaviour
{
    private static InitScenePacker _active;
    public static InitScenePacker Active
    {
        get
        {
            if (_active == null)
                _active = FindObjectOfType<InitScenePacker>();
            return _active;
        }
    }
    //all components that make the scene init packet
    public List<SceneInitComponent> sceneInitComponents = new List<SceneInitComponent>();
    
    //all clients waiting for a scene init response
    private Dictionary<int, System.Action> sceneInitResponseFunctionality = new Dictionary<int, Action>();

    //==> SERVER
    //get notified when a new client joins the game
    public void InitClientScene(int _client, Action _onInitComplete)
    {
        Packet _packet = new Packet((int)ServerPackets.initScene);
        //ask all init components to prepare their data
        //<==Scene Initializer Count
        _packet.Write(sceneInitComponents.Count);
        for (int i = 0; i < sceneInitComponents.Count; i++)
        {
            //<== SUBPACKET ID
            _packet.Write(sceneInitComponents[i].SubPacketID());
            sceneInitComponents[i].GetSubPacket().Write(_packet);
        }           
        
        //save the response functionality
        if (sceneInitResponseFunctionality.ContainsKey(_client))
            sceneInitResponseFunctionality.Remove(_client);
        sceneInitResponseFunctionality.Add(_client, _onInitComplete);
        //send the data over the network
        ServerSend.SendTCPData(_client, _packet);
        
        Debug.Log("sent scene init packet; " + _client + "using id " + (int)ServerPackets.initScene );
        
    }

    /// <summary>
    /// run th saved functionality for scene init responses
    /// </summary>
    public void HandleSceneInitResponse(int _client)
    {
        sceneInitResponseFunctionality[_client].Invoke();
        sceneInitResponseFunctionality.Remove(_client);
    }

    //==>CLIENT
    //get notified when a scene init function comes in
    //send the data to the respective components that need it

}

public interface SceneInitComponent
{
    /// <summary>
    /// the id to be used as a head for this sub packet
    /// </summary>
    int SubPacketID();
    /// <summary>
    /// the data this component would like to be added to the scene init packet
    /// </summary>
    SubPacket GetSubPacket();

    /// <summary>
    /// tell this SceneInitComponent To Handle A SubPacket 
    /// </summary>
    void HandleSubPacket(Packet _packet);
}
