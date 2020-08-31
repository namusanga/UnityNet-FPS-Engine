using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// used to make local input and transmit it to server
/// </summary>
public class AuthCharInput : MonoBehaviour
{
    public static bool simulated = false;
    public event Action shootAction;
    public int currentInput = 0;

    [HideInInspector] public CharacterShooter shooter;

    private List<CharacterInput> inputBuffer;
    private AuthoritativeCharacter character;
    private CharacterPredictor predictor;
    private Vector2 simVector;

    void Awake()
    {
        inputBuffer = new List<CharacterInput>();
        character = GetComponent<AuthoritativeCharacter>();
        predictor = GetComponent<CharacterPredictor>();
        if (simulated)
        {
            simVector.x = Random.Range(0, 1) > 0 ? 1 : -1;
            simVector.y = Random.Range(-1f, 1f);
        }
    }

    private float MouseLookX;
    private bool shoot;
    private int hitPlayerLocalTick;
    private int hitPlayer;
    void Update()
    {
        //GET INPUT
        Vector2 movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        MouseLookX = Input.GetAxis("Mouse X");

        if (shooter)
            shoot = shooter.CheckShot(Input.GetMouseButton(0), out hitPlayer ,out hitPlayerLocalTick);

        //dont add anything if no input is being pressed
        if (inputBuffer.Count == 0 && movementInput == Vector2.zero && MouseLookX == 0 && shoot == false)
            return;

        #region PROCESSING INPUT
        //make the player look
        transform.Rotate(new Vector3(0, MouseLookX * character.LookSpeed, 0));

        if (shoot)
        {
            //make a new input
            CharacterInput charInput = new CharacterInput(movementInput, currentInput++, Vector3.zero, transform.rotation, shoot, transform.rotation, hitPlayerLocalTick, hitPlayer);

            //send to predictor
            Vector3 _position = predictor.AddInput(charInput);

            //save the predicted position
            charInput.predictedResult = _position;
            inputBuffer.Add(charInput);

            //send the input right away
            character.ServerMove(GetInputBuffer());
        }
        else
        {
            //make a new input
            CharacterInput charInput = new CharacterInput(movementInput, currentInput++, Vector3.zero, transform.rotation, false, Quaternion.identity, 0,0);

            //send to predictor
            Vector3 _position = predictor.AddInput(charInput);

            //save the predicted position
            charInput.predictedResult = _position;
            inputBuffer.Add(charInput);
        }
        #endregion
    }

    private bool imediateSendInputs = false;
    void FixedUpdate()
    {
        if (imediateSendInputs)
        {
            character.ServerMove(GetInputBuffer());
            imediateSendInputs = false;
            return;
        }

        //if our input buffer has not reached the needed number of inputs, dont send it to the server
        if (inputBuffer.Count < character.InputBufferSize)
            return;

        //if it has reached
        //send to server
        character.ServerMove(GetInputBuffer());
    }

    public CharacterInput[] GetInputBuffer(bool _clearBuffer = true)
    {
        List<CharacterInput> copy = new List<CharacterInput>(inputBuffer);

        if (_clearBuffer)
            inputBuffer.Clear();

        return copy.ToArray();
    }

}