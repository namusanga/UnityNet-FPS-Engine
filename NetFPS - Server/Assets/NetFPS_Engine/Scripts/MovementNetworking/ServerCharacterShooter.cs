using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Server-side shooter logic
/// </summary>

[RequireComponent(typeof(AuthoritativeCharacter))]
public class ServerCharacterShooter : MonoBehaviour
{
    public Transform shootPoint;

    private void Start()
    {
        GetComponent<AuthoritativeCharacter>().shooter = this;
    }


    public bool CheckShot(Collider collider)
    {
        if(Physics.Raycast(shootPoint.position, shootPoint.forward, out RaycastHit _hit))
        {
            if (_hit.collider == collider)
                return true;
        }
        return false;
    }
}

