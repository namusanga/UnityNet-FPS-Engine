using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

/// <summary>
/// used to make local input and transmit it to server
/// </summary>
public class AuthCharInput : MonoBehaviour
{
    public static bool simulated = false;

    List<CharacterInput> inputBuffer;
    AuthoritativeCharacter character;
    AuthCharPredictor predictor;
    public int currentInput = 0;
    Vector2 simVector;

    void Awake()
    {
        inputBuffer = new List<CharacterInput>();
        character = GetComponent<AuthoritativeCharacter>();
        predictor = GetComponent<AuthCharPredictor>();
        if (simulated)
        {
            simVector.x = Random.Range(0, 1) > 0 ? 1 : -1;
            simVector.y = Random.Range(-1f, 1f);
        }
    }

    private float MouseLookX;
    void Update()
    {
        Vector2 movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        MouseLookX = Input.GetAxis("Mouse X");

        //dont add anything if no input is being pressed
        if (inputBuffer.Count == 0 && movementInput == Vector2.zero && MouseLookX == 0)
            return;

        //make the player look
        transform.Rotate(new Vector3(0, MouseLookX * character.LookSpeed, 0));

        //make a new input
        CharacterInput charInput = new CharacterInput(movementInput, currentInput++, Vector3.zero, transform.rotation);

        //send to predictor
        Vector3 _position = predictor.AddInput(charInput);

        //save the predicted position
        charInput.predictedResult = _position;
        inputBuffer.Add(charInput);
    }

    void FixedUpdate()
    {
        //if our input buffer has not reached the needed number of inputs, dont send it to the server
        if (inputBuffer.Count < character.InputBufferSize)
            return;

        //if it has reached
        //send to server
        character.ServerMove(inputBuffer.ToArray());
        //refresh the buffer
        inputBuffer.Clear();
    }


    Vector2 SimulatedVector()
    {
        if (transform.position.x > 5)
            simVector.x = Random.Range(-1f, 0);
        else if (transform.position.x < -5)
            simVector.x = Random.Range(0, 1f);
        if (transform.position.z > 2 || transform.position.z < -2)
            simVector.y = 0;
        return simVector;
    }
}