using UnityEngine;

public class DartBehavior : MonoBehaviour
{
    public gameController gameController;
    private bool hasScored = false;
    private Rigidbody rb;
    private Collider dartCollider;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        dartCollider = GetComponent<Collider>();
        
        // Auto-find game controller if not assigned
        if (gameController == null)
        {
            gameController = FindObjectOfType<gameController>();
        }
        
        Destroy(gameObject, 10f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasScored) return;

        if (collision.gameObject.CompareTag("Dartboard"))
        {
            hasScored = true;
            
            // Stop physics and stick to board
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            
            transform.SetParent(collision.transform);
            
            if (dartCollider != null)
            {
                dartCollider.enabled = false;
            }
            
            // CALL THE NEW SCORING METHOD - CRITICAL!
            if (gameController != null)
            {
                gameController.DartHit(transform.position);
            }
        }
        else
        {
            hasScored = true;
            if (gameController != null)
            {
                gameController.DartMissed();
            }
            Destroy(gameObject, 2f);
        }
    }
}