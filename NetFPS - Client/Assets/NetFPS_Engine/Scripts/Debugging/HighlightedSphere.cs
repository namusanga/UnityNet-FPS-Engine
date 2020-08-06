using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightedSphere : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, .2f);
        Gizmos.color = Color.white;
    }
}
