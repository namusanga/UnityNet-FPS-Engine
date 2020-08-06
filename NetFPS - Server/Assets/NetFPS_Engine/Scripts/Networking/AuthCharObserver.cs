using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// used to observe the state of a remote player on the server
/// </summary>
public class AuthCharObserver : MonoBehaviour, IAuthCharStateHandler
{
    LinkedList<CharacterState> stateBuffer;
    AuthoritativeCharacter character;
    public int clientTick = 0;

    void Awake()
    {
        character = GetComponent<AuthoritativeCharacter>();
        stateBuffer = new LinkedList<CharacterState>();
        SetObservedState(character.state);
        AddState(character.state);
    }

    void Update()
    {
        int pastTick = clientTick - character.interpolationDelay;
        var fromNode = stateBuffer.First;
        var toNode = fromNode.Next;
        //if we have a node to interpolate to 
        //and that nodes time is greater than or equal to our last tick
        while (toNode != null && toNode.Value.timestamp <= pastTick)
        {
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

    public void OnStateChange(CharacterState newState)
    {
        clientTick = newState.timestamp;
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
        character.SyncState(newState);
    }
}