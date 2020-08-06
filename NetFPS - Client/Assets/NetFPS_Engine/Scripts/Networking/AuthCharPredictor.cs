using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AuthCharPredictor : MonoBehaviour, IAuthCharStateHandler
{
    public GameObject serverPositionSphere;

    LinkedList<CharacterInput> pendingInputs;//the inputs we have to simulate
    AuthoritativeCharacter character;//the character
    CharacterState predictedState;//the state we have predicted
    private CharacterState lastServerState = CharacterState.Zero;//the last server state

    void Awake()
    {
        pendingInputs = new LinkedList<CharacterInput>();
        character = GetComponent<AuthoritativeCharacter>();
    }

    /// <summary>
    /// simulate the input locally and store it
    /// until the server has an update that's later than that input
    /// </summary>
    /// <param name="input"></param>
    public Vector3 AddInput(CharacterInput input)
    {
        //predict the outcome
        ApplyInput(input, out Vector3 _position, out Quaternion _rotation);

        input.predictedResult = _position;
        pendingInputs.AddLast(input);


        character.SyncPosition(predictedState);//make the move phyisically
        predictedState.position = character.transform.position;//save the position we get back from the character controller

        //return the result of the calculation
        return _position;
    }


    /// <summary>
    /// when the server state changes
    /// called by the AuthoritativeCharacter
    /// </summary>
    /// <param name="newState"></param>
    public void OnServerUpdate(CharacterState newState)
    {
        //if the timestamp is older than the last timestamp we got from the server,
        //skip it udp failure
        if (newState.timestamp <= lastServerState.timestamp)
        {
            Debug.LogError("UDP failure");
            print($"Faile :: New Server State:: {newState.timestamp} Old State time :: {lastServerState.timestamp}");
            return;
        }

        print($"New Server State:: {newState.timestamp} Old State time :: {lastServerState.timestamp}");

        //remove any states that are older than the servers last update state
        while (pendingInputs.Count > 0 && pendingInputs.First().inputNum <= newState.moveNum)
        {
            pendingInputs.RemoveFirst();
        }

        //change our state to that of the server
        predictedState = newState;
        lastServerState = newState;

        character.SyncPosition(lastServerState);

        //predict any new states that need to be predicted
        UpdatePredictedState();
    }


    //get all new inputs and add them
    void UpdatePredictedState()
    {
        foreach (CharacterInput input in pendingInputs)
        {
            ApplyInput(input, out Vector3 vector3, out Quaternion _rotation);
        }
        character.SyncPosition(predictedState);
    }

    /// <summary>
    /// change our predicted state to the new input
    /// </summary>
    /// <param name="input"></param>
    void ApplyInput(CharacterInput input, out Vector3 _position, out Quaternion _rotation)
    {
        predictedState = CharacterState.Move(predictedState, input, character.Speed, 0, transform);
        _position = predictedState.position;
        _rotation = predictedState.rotation;
    }

}