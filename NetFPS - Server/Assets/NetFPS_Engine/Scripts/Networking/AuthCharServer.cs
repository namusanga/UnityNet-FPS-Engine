using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AuthCharServer : MonoBehaviour
{
    Queue<CharacterInput> inputBuffer;
    AuthoritativeCharacter character;
    int movesMade;
    int serverTick;

    CharacterController charCtrl;
    public Player player;

    public float delayForSelfUpdate = 5;

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
            while ((movesMade < character.InputBufferSize && inputBuffer.Count > 0))
            {
                //change the character state using the input
                state = CharacterState.Move(state, inputBuffer.Dequeue(), character.Speed, serverTick, transform);
                charCtrl.Move(state.position - charCtrl.transform.position);//make the physical character move according to the input
                state.position = charCtrl.transform.position;
                movesMade++;//increase the number of made moves
            }

            //if the made moves are greater than 0
            //then we have made some moves, let the character controller know
            if (movesMade > 0)
            {
                state.position = transform.position;//set our position
                character.state = state;//save our state
                //send to other characters of this sort

                if(delayForSelfUpdate > 0)
                {
                    //dont self update
                    BroadCastPlayerState(player.id,state, true);
                }else
                {
                    BroadCastPlayerState(player.id, state, false);
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

    /// <summary>
    /// let all other clients know about the change
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="_state"></param>
    public static void BroadCastPlayerState(int playerID, CharacterState _state, bool _excludeSelf)
    {
        using (Packet _packet = new Packet((int)ServerPackets.BroadcastPlayerState))
        {
            _packet.Write(playerID);
            _state.WritePacket(_packet);

            if(_excludeSelf)
            ServerSend.SendUDPDataToAll(playerID,_packet);
            else
            ServerSend.SendUDPDataToAll(_packet);
        }
    }


    public static void BroadcastAllPlayerStates()
    {
        for (int i = 0; i < AuthoritativeCharacter.serverPlayers.Count; i++)
        {
            BroadCastPlayerState(AuthoritativeCharacter.serverPlayers[i].server.player.id, AuthoritativeCharacter.serverPlayers[i].state, false);
        }
    }
}