using System.Collections.Generic;
using UnityEngine;

public class ServerCharacter : MonoBehaviour
{
    Queue<CharacterInput> inputBuffer;
    AuthoritativeCharacter character;
    int movesMade;
    int serverTick;

    CharacterController charCtrl;
    public Player player;

    public float delayForSelfUpdate = 5;

    public List<CharacterState> myPreviousStates = new List<CharacterState>();

    void Awake()
    {
        inputBuffer = new Queue<CharacterInput>();
        character = GetComponent<AuthoritativeCharacter>();
        character.state = CharacterState.Zero;
        charCtrl = GetComponent<CharacterController>();
        player = GetComponent<Player>();
    }

    bool playerHasJustStarted = true;
    void Update()
    {
        delayForSelfUpdate -= Time.deltaTime;

        if (movesMade > 0)//if greater than zero
            movesMade--;//reduce the number of made moves

        if (movesMade == 0)//if no new moves have been made
        {
            CharacterState state = character.state;//get reference to our state
            //while movemade is less than the inpubuffer available
            //and there is an input buffer
            //aka loop until there is no more moves to make
            //loop through any available moves in the input buffer and make them
            while (movesMade < character.InputBufferSize && inputBuffer.Count > 0)
            {
                CharacterInput _input = inputBuffer.Dequeue();

                if (_input.shoot)
                {
                    //get the shot player
                    AuthoritativeCharacter shotCharacter = AuthoritativeCharacter.serverPlayers.Find(x => x.myPlayer.id == _input.shotPlayer);
                    //move him back to the former position
                    Vector3 currentPlayerPosition = shotCharacter.transform.position;
                    CharacterState _ShotPlayer_lastSentState = shotCharacter.server.GetLastSentStateFromTimeStamp(_input.localTickForShotPlayer);

                    int previousCharcaterStateIndex = shotCharacter.server.myPreviousStates.IndexOf(_ShotPlayer_lastSentState) - 1;

                    previousCharcaterStateIndex = Mathf.Clamp(previousCharcaterStateIndex,0, shotCharacter.server.myPreviousStates.Count-1);

                    shotCharacter.transform.position = CharacterState.Interpolate(_ShotPlayer_lastSentState, shotCharacter.server.myPreviousStates[previousCharcaterStateIndex], _input.localTickForShotPlayer).position;
                    //perform the raycast
                    if (character.shooter.CheckShot(shotCharacter.transform.Find("Capsule").GetComponent<Collider>())){
                        //broadcast the hit to the other players
                        BroadCastPlayerHit(player.id, shotCharacter.myPlayer.id);
                    }
                    //return the player back to his normal position
                    shotCharacter.transform.position = currentPlayerPosition;
                }

                //change the character state using the input
                state = CharacterState.Move(state, _input, character.Speed, serverTick, transform);
                charCtrl.Move(state.position - charCtrl.transform.position);//make the physical character move according to the input
                state.position = charCtrl.transform.position;
                movesMade++;//increase the number of made moves
            }

            //if the made moves are greater than 0
            //then we have made some moves, let the character controller know
            if (movesMade > 0)
            {
                SaveState(state);
                state.position = transform.position;//set our position
                character.state = state;//save our state
                //send to other characters of this sort

                if (delayForSelfUpdate > 0)
                {
                    //dont self update
                    BroadCastPlayerState(character, state, true);

                }
                else
                {
                    BroadCastPlayerState(character, state, false);
                    delayForSelfUpdate = 5;
                }
            }
        }

        if (playerHasJustStarted)
        {
            BroadcastAllPlayerStates();
            playerHasJustStarted = false;
        }
    }


    void FixedUpdate()
    {
        serverTick++;
    }

    //adds the sent controls to the input buffer
    public void Move(CharacterInput[] inputs)
    {
        foreach (var input in inputs)
            inputBuffer.Enqueue(input);
    }

    public CharacterState GetLastSentStateFromTimeStamp(int _timeStamp)
    {
        for (int i = myPreviousStates.Count - 1; i < 0; i--)
        {
            if (myPreviousStates[i].timestamp <= _timeStamp)
                return myPreviousStates[i];
        }
        return myPreviousStates[myPreviousStates.Count - 1];
    }

    public void SaveState(CharacterState _state)
    {
        if (myPreviousStates.Count >= 100)
            myPreviousStates.RemoveAt(0);

        myPreviousStates.Add(_state);
    }

    #region Static Hanlders

    /// <summary>
    /// let all other clients know about the change
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="_state"></param>
    public static void BroadCastPlayerState(AuthoritativeCharacter _player, CharacterState _state, bool _excludeSelf)
    {
        //save this state
        _player.server.SaveState(_state);

        using (Packet _packet = new Packet((int)ServerPackets.BroadcastPlayerState))
        {
            _packet.Write(_player.myPlayer.id);
            _state.WritePacket(_packet);

            if (_excludeSelf)
                ServerSend.SendUDPDataToAll(_player.myPlayer.id, _packet);
            else
                ServerSend.SendUDPDataToAll(_packet);
        }
    }


    public static void BroadcastAllPlayerStates()
    {
        for (int i = 0; i < AuthoritativeCharacter.serverPlayers.Count; i++)
        {
            BroadCastPlayerState(AuthoritativeCharacter.serverPlayers[i], AuthoritativeCharacter.serverPlayers[i].state, false);
        }
    }

    public static void BroadCastPlayerHit(int _shootingPlayer, int _shotPlayer)
    {
        using (Packet _packet = new Packet((int)ServerPackets.PlayerShot))
        {
            _packet.Write(_shootingPlayer);
            _packet.Write(_shotPlayer);

            ServerSend.SendUDPDataToAll(_packet);
        }
    }
    #endregion
}