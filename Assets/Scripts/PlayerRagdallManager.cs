using UnityEngine;

public class PlayerRagdallManager : MonoBehaviour
{
    void ResetPose()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }
}
