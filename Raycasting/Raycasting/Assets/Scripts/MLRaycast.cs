using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class MLRaycast : MonoBehaviour {

    public Transform ctransform; // Camera's transform
    public GameObject prefab;

    // Use this for initialization
    void Start ()
    {
        MLWorldRays.Start();
    }
	
	// Update is called once per frame
	void Update () {
        MLWorldRays.QueryParams _raycastParams = new MLWorldRays.QueryParams
        {
            // Update the parameters with our Camera's transform
            Position = ctransform.position,
            Direction = ctransform.forward,
            UpVector = ctransform.up,
            // Provide a size of our raycasting array (1x1)
            Width = 1,
            Height = 1
        };
        // Feed our modified raycast parameters and handler to our raycast request
        MLWorldRays.GetWorldRays(_raycastParams, HandleOnReceiveRaycast);

    }

    void HandleOnReceiveRaycast(MLWorldRays.MLWorldRaycastResultState state, UnityEngine.Vector3 point, UnityEngine.Vector3 normal, float confidence)
    {
        if (state == MLWorldRays.MLWorldRaycastResultState.HitObserved)
        {
            StartCoroutine(NormalMarker(point, normal));
        }
    }

    private void OnDestroy()
    {
        MLWorldRays.Stop();
    }

    private IEnumerator NormalMarker(Vector3 point, Vector3 normal)
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
        GameObject go = Instantiate(prefab, point, rotation);
        yield return new WaitForSeconds(2);
        Destroy(go);
    }

}
