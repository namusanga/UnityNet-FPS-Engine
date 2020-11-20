using System.Collections;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// handles shot checking and responses when players get shot
/// </summary>
[RequireComponent(typeof(AuthoritativeCharacter))]
public class CharacterShooter : MonoBehaviour
{
    public Transform shootPoint;
    public LayerMask shootLayers;
    public bool localPlayer;

    private AuthoritativeCharacter mcharacter;
    private AuthCharInput minput;
    private PlayerInfo playerInfo;
    private void OnEnable()
    {
        if (localPlayer)
        {
            minput = GetComponent<AuthCharInput>();
            minput.shooter = this;
        }

        mcharacter = GetComponent<AuthoritativeCharacter>();
        mcharacter.shooter = this;
        playerInfo = GetComponent<PlayerInfo>();
    }

    /// <summary>
    /// checks if another player has been shot
    /// </summary>
    public bool CheckShot(bool _shoot, out int hitPlayer, out int localTickForHitPlayer)
    {

        if (_shoot)
        {
            Debug.DrawLine(shootPoint.transform.position, shootPoint.forward * 1000, Color.red);
            if (Physics.Raycast(shootPoint.transform.position, shootPoint.forward, out RaycastHit _hit))
            {
                var observer = _hit.collider.GetComponentInParent<RemoteCharacterObsever>();
                if (observer)
                {
                    //hit player
                    hitPlayer = observer.character.player.id;
                    localTickForHitPlayer = observer.GetCurrentInpterploationTick();
                    return true;
                }
            }
        }

        //faield to hit anything
        localTickForHitPlayer = 0;
        hitPlayer = 0;
        return false;
    }

    /// <summary>
    /// my player just got shot
    /// </summary>
    public void PlayerShot(int shootingPlayer, int shotPlayer)
    {
        if (shotPlayer == Client.Active.myId)
        {
            StartCoroutine(ShowHitAnimation());
        }
        else if (shotPlayer == playerInfo.id)
        {
            ChangeColor();
        }
    }

    public IEnumerator ShowHitAnimation()
    {
        Image _hitObject = GameObject.Find("HitObject").GetComponent<Image>();
        _hitObject.enabled = true;
        yield return new WaitForSeconds(2);
        _hitObject.enabled = false;
    }

    public void ChangeColor()
    {
        transform.Find("Capsule").GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
    }
}
