using UnityEngine;

public class HandCatcher : MonoBehaviour
{
    [Header("Hand Settings")]
    public bool isLeftHand = true;

    [Header("Catch Detection")]
    public float catchRadius = 0.08f;
    public string palmBoneName = ""; // Will auto-detect if empty

    private GameObject palmCatcherObject;
    private SphereCollider catchCollider;
    private Transform palmBone;
    private bool isSetup = false;

    void Start()
    {
        // Wait a frame for OVRHand to generate bones
        Invoke(nameof(SetupCatchCollider), 0.5f);
    }

    void SetupCatchCollider()
    {
        // Try to find the palm bone
        palmBone = FindPalmBone();

        if (palmBone != null)
        {
            Debug.Log($"{(isLeftHand ? "Left" : "Right")} hand: Found palm bone at {palmBone.name}");

            // Create a catcher object as child of palm bone
            palmCatcherObject = new GameObject($"{(isLeftHand ? "Left" : "Right")}PalmCatcher");
            palmCatcherObject.transform.SetParent(palmBone);
            palmCatcherObject.transform.localPosition = Vector3.zero;
            palmCatcherObject.transform.localRotation = Quaternion.identity;

            // Add sphere collider for catching
            catchCollider = palmCatcherObject.AddComponent<SphereCollider>();
            catchCollider.isTrigger = true;
            catchCollider.radius = catchRadius;

            // Add rigidbody for trigger detection
            Rigidbody rb = palmCatcherObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Tag as Hand
            palmCatcherObject.tag = "Hand";

            // Add collision handler
            PalmCollisionHandler handler = palmCatcherObject.AddComponent<PalmCollisionHandler>();
            handler.isLeftHand = isLeftHand;

            isSetup = true;
        }
        else
        {
            Debug.LogWarning($"{(isLeftHand ? "Left" : "Right")} hand: Could not find palm bone. Falling back to hand anchor.");
            // Fallback: attach to hand anchor directly
            SetupFallbackCatcher();
        }
    }

    Transform FindPalmBone()
    {
        // Search for common palm/wrist bone names
        string[] boneNames;

        if (string.IsNullOrEmpty(palmBoneName))
        {
            // Auto-detect based on hand type
            boneNames = isLeftHand
                ? new string[] { "hands:b_l_wrist", "b_l_wrist", "l_wrist", "LeftWrist" }
                : new string[] { "hands:b_r_wrist", "b_r_wrist", "r_wrist", "RightWrist" };
        }
        else
        {
            boneNames = new string[] { palmBoneName };
        }

        // Search in children
        foreach (string boneName in boneNames)
        {
            Transform bone = transform.FindDeepChild(boneName);
            if (bone != null)
            {
                return bone;
            }
        }

        return null;
    }

    void SetupFallbackCatcher()
    {
        // If we can't find bones, attach directly to hand anchor
        catchCollider = gameObject.AddComponent<SphereCollider>();
        catchCollider.isTrigger = true;
        catchCollider.radius = catchRadius;

        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        gameObject.tag = "Hand";

        PalmCollisionHandler handler = gameObject.AddComponent<PalmCollisionHandler>();
        handler.isLeftHand = isLeftHand;

        isSetup = true;
        Debug.Log($"{(isLeftHand ? "Left" : "Right")} hand: Using fallback catcher on hand anchor");
    }

    // Visual debug
    void OnDrawGizmos()
    {
        if (isSetup && palmCatcherObject != null)
        {
            Gizmos.color = isLeftHand ? new Color(0, 0, 1, 0.3f) : new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(palmCatcherObject.transform.position, catchRadius);
        }
        else
        {
            Gizmos.color = isLeftHand ? new Color(0, 0, 1, 0.3f) : new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, catchRadius);
        }
    }
}

// Helper class to handle collisions
public class PalmCollisionHandler : MonoBehaviour
{
    public bool isLeftHand = true;

    void OnTriggerEnter(Collider other)
    {
        FoodItem food = other.GetComponent<FoodItem>();
        if (food != null)
        {
            Debug.Log($"{(isLeftHand ? "Left" : "Right")} hand caught food!");
            food.AttachToHand(transform);
        }
    }
}

// Extension method to search deep in hierarchy
public static class TransformExtensions
{
    public static Transform FindDeepChild(this Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = child.FindDeepChild(name);
            if (result != null)
                return result;
        }
        return null;
    }
}