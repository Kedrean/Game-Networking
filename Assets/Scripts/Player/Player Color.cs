using UnityEngine;
using Unity.Netcode;

public class PlayerColor : NetworkBehaviour
{
    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (OwnerClientId == 0)
            sr.color = Color.blue;
        else
            sr.color = Color.red;
    }
}