using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class GestureScript : MonoBehaviour
{
    private GameObject cube; // Reference to our Cube
    private float distance = 2.0f; // Distance between Main Camera and the cube
    private MLHandKeyPose[] gestures; // Holds the different hand poses we will look for

    private bool OKHandPose = false;
    private float speed = 30.0f; // Speed of our cube

    // Use this for initialization
    void Start()
    {
        MLHands.Start(); // Start the hand tracking.

        gestures = new MLHandKeyPose[4]; //Assign the gestures we will look for.
        gestures[0] = MLHandKeyPose.Ok;
        gestures[1] = MLHandKeyPose.Fist;
        gestures[2] = MLHandKeyPose.OpenHandBack;
        gestures[3] = MLHandKeyPose.Finger;
        // Enable the hand poses.
        MLHands.KeyPoseManager.EnableKeyPoses(gestures, true, false);

        cube = GameObject.Find("Cube"); // Find our Cube in the scene.
        cube.SetActive(false);
    }

    void OnDestroy()
    {
        MLHands.Stop();
    }


    void Update()
    {
        if (OKHandPose)
        {
            if (GetGesture(MLHands.Left, MLHandKeyPose.OpenHandBack)
                || GetGesture(MLHands.Right, MLHandKeyPose.OpenHandBack))
                cube.transform.Rotate(Vector3.up, +speed * Time.deltaTime);

            if (GetGesture(MLHands.Left, MLHandKeyPose.Fist)
                || GetGesture(MLHands.Right, MLHandKeyPose.Fist))
                cube.transform.Rotate(Vector3.up, -speed * Time.deltaTime);

            if (GetGesture(MLHands.Left, MLHandKeyPose.Finger))
                cube.transform.Rotate(Vector3.right, +speed * Time.deltaTime);

            if (GetGesture(MLHands.Right, MLHandKeyPose.Finger))
                cube.transform.Rotate(Vector3.right, -speed * Time.deltaTime);
        } else
        {
            if (GetGesture(MLHands.Left, MLHandKeyPose.Ok)
                || GetGesture(MLHands.Right, MLHandKeyPose.Ok))
            {
                OKHandPose = true;
                cube.SetActive(true);
                cube.transform.position = transform.position + transform.forward * distance;
                cube.transform.rotation = transform.rotation;
            }
        }
    }

    bool GetGesture(MLHand hand, MLHandKeyPose type)
    {
        if (hand != null)
        {
            if (hand.KeyPose == type)
            {
                if (hand.KeyPoseConfidence > 0.9f)
                {
                    return true;
                }
            }
        }
        return false;
    }
}