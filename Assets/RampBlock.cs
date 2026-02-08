//using UnityEngine;

//public class RampBlock : Block
//{
//    [Header("Ramp Settings")]
//    public float rampAngle = 45f;
//    public Vector3 rampDirection = Vector3.forward;

//    void Start()
//    {
//        blockType = BlockType.Ramp;

//        // Set ramp rotation
//        transform.rotation = Quaternion.Euler(0, 0, rampAngle);
//    }

//    // Override connection check for ramp angles
//    protected override bool IsTouching(Block otherBlock)
//    {
//        // Ramps might connect differently (sloped surfaces)
//        float touchThreshold = 0.15f; // More tolerance for ramps
//        float distance = Vector3.Distance(transform.position, otherBlock.transform.position);

//        return distance <= 1.0f + touchThreshold;
//    }
//}