using UnityEngine;

public class Killbox : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debug;
    
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = debug;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Player>(out Player player))
        {
            player.Die();
        }
        if (other.TryGetComponent<Egg>(out Egg egg))
        {
            egg.Break(true);
        }
    }

    private void OnDrawGizmos()
    {
        if (!debug) return;
        // Set the color to red for visibility
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        
        // Draw a wire cube at the transform's position using the collider's size
        if (TryGetComponent<BoxCollider>(out BoxCollider collider))
        {
            // Draw box using the collider's center offset and size, transformed by the object's transform
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(collider.center, collider.size);
        }
    }
}
