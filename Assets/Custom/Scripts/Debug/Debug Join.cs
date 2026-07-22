using UnityEngine;

public class DebugJoin : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            MultiplayerManager.Instance.HostGame();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("Paste Join Code Here");
        }
    }
}