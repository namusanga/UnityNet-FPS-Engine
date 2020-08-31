using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// used to observe the state of a remote player on the server
/// </summary>
public class RemoteCharacterObsever : MonoBehaviour, IAuthCharStateHandler
{
    LinkedList<CharacterState> stateBuffer;
    [HideInInspector] public AuthoritativeCharacter character;
    public int clientTick = 0;//the current client tick according to the server


    void Awake()
    {
        character = GetComponent<AuthoritativeCharacter>();
        stateBuffer = new LinkedList<CharacterState>();
        SetObservedState(character.state);
        AddState(character.state);
    }

    /// <summary>
    /// get the current interpolation tick for the character's position
    /// </summary>
    /// <returns></returns>
    public int GetCurrentInpterploationTick()
    {
        return clientTick - character.interpolationDelay;
    }

    void Update()
    {
        int pastTick = clientTick - character.interpolationDelay;
        var fromNode = stateBuffer.First;//the node we are moving from
        var toNode = fromNode.Next;//the next node in the buffer


        //1. we have any new node to interpolate to 
        //2. the next node is earlier thatn the last update we got
        while (toNode != null && toNode.Value.timestamp <= pastTick)
        {
            //skip the to node
            fromNode = toNode;
            toNode = fromNode.Next;
            stateBuffer.RemoveFirst();
        }

        SetObservedState(toNode != null ? CharacterState.Interpolate(fromNode.Value, toNode.Value, pastTick) : fromNode.Value);
    }

    void FixedUpdate()
    {
        clientTick++;
    }

    public void OnServerUpdate(CharacterState newState)
    {
        clientTick = newState.timestamp;//save the sevrer timestep of this client
        AddState(newState);
    }

    void AddState(CharacterState state)
    {
        if (stateBuffer.Count > 0 && stateBuffer.Last.Value.timestamp > state.timestamp)
        {
            return;
        }
        stateBuffer.AddLast(state);
    }

    void SetObservedState(CharacterState newState)
    {
        character.SyncPosition(newState);
        character.SyncRotation(newState);
    }

}