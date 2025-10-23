using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class FoodItem : MonoBehaviour
{
    [HideInInspector]
    public FoodSpawner spawner;

    private bool isCaught = false;
    private bool hasHitFloor = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Ensure rigidbody is set up correctly
        rb.useGravity = true;
        rb.mass = 0.1f;

        // Ensure collider is not a trigger (for physics)
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if hit the floor
        if (collision.gameObject.CompareTag("Floor") && !isCaught)
        {
            if (!hasHitFloor)
            {
                hasHitFloor = true;
                Debug.Log("Food hit the floor! Game Over!");

                // Stop the game
                if (spawner != null)
                {
                    spawner.StopSpawning();
                }

                // Destroy this food after a delay
                Destroy(gameObject, 2f);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if caught by hand
        if (other.CompareTag("Hand") && !isCaught && !hasHitFloor)
        {
            CatchFood(other.gameObject);
        }
    }

    void CatchFood(GameObject hand)
    {
        isCaught = true;
        Debug.Log("Food caught by: " + hand.name);

        // Optional: Attach to hand temporarily
        transform.SetParent(hand.transform);
        rb.isKinematic = true;

        // Destroy after a short time
        Destroy(gameObject, 1f);
    }

    // Alternative catching method if you want to parent to hand
    public void AttachToHand(Transform handTransform)
    {
        if (!isCaught)
        {
            isCaught = true;
            transform.SetParent(handTransform);
            rb.isKinematic = true;
            Destroy(gameObject, 1f);
        }
    }
}