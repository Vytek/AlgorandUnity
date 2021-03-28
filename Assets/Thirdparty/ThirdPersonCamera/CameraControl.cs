using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{

#region Editor Fields
    //Serialize field lets us edit in editor window without making variables public
    [SerializeField]
    [Tooltip("The transform of the player object")]
    Transform player;

    [SerializeField]
    [Tooltip("The transform of the player's target")]
    Transform target;

    [SerializeField]
    [Tooltip("The distance from the LookAt target for the camera to be")]
    float cameraDistance;

    [SerializeField]
    [Tooltip("The scalar to divide the camera distance by when getting closer to the target. Higher number will zoom in more")]
    float distanceScalar;

    [SerializeField]
    [Tooltip("Camera movement sensitivity")]
    float mouseSensitivity;

    [SerializeField]
    [Tooltip("LayerMask for obscuring objects")]
    LayerMask cameraCollisionLayerMask;

    [SerializeField]
    [Tooltip("Offset to place camera in front of occluding objects")]
    float rayCastOffset;

    [SerializeField]
    [Tooltip("Max distance to right of player in targeted mode")]
    float maxHorizontalOffset;

    [SerializeField]
    [Tooltip("Max distance to left of player in targeted mode (should be negative)")]
    float minHorizontalOffset;

    [SerializeField]
    [Tooltip("Max distance above player in targeted mode")]
    float maxVerticalOffset;

    [SerializeField]
    [Tooltip("Max distance below player in targeted mode (should be negative)")]
    float minVerticalOffset;

    [SerializeField]
    [Tooltip("Time in seconds for camera transition between modes")]
    float camTransitionTime;

    [SerializeField]
    [Tooltip("Whether or not to invert the camera's Y-Axis")]
    bool inverted;
#endregion

#region Private Variables
    float alpha;
    float theta;
    float horizontalOffset;
    float verticalOffset;
    Vector3 lastMousePosition;
    bool transitioning;
#endregion

    /*
     * Summary: 
     *      Handles setting current target and initialzing transitions
     * Parameters:
     *      newTarget:
     *          Transform of the new target for the camera. If null, reverts to a normal third person camera
     */
    public void SetTarget(Transform newTarget) {

        // Targeted camera initialization
        if(newTarget != null) {
            target = newTarget;
            horizontalOffset = 0;
            verticalOffset = 0;

            // Calculate Camera vectors for positioning
            Vector3 targetVector = target.transform.position - player.transform.position;
            Vector3 cameraRightVector = Vector3.Cross(Vector3.up, targetVector.normalized);

            // Get targeted camera position
            Vector3 camLocationTargeted = CameraLocationTargeted(target);

            // Initialize targeted camera offset by projecting the camera location vector onto player transform vectors
            Vector3 cameraVector = transform.position - camLocationTargeted;
            horizontalOffset = Vector3.Dot(player.transform.right, cameraVector);
            verticalOffset = Vector3.Dot(player.transform.up, cameraVector);

            // Clamp to keep camera within bounds set by offsets
            ClampCameraPosition();

            // Start transition routine and return
            StartCoroutine(TransitionCameraToTarget(target));
            return;
        }

        // Thirdperson camera initialization

        // Initialize vectors we'll need for calculations
        Vector3 playerVector = transform.position - player.transform.position;
        Vector3 alphaVector = playerVector;
        Vector3 thetaVector = playerVector;
        alphaVector.y = 0;
        alphaVector.Normalize();
        thetaVector.Normalize();

        // Calculate the thirdperson alpha and theta from the appropriate vectors
        // Determine our alpha with the dot product
        alpha = Mathf.Acos(Vector3.Dot(alphaVector, Vector3.forward));
        // Depending on our quadrant, update our alpha
        if(Vector3.Dot(alphaVector, Vector3.right) > 0.0f) {
            alpha = -alpha;
        }
        // Offset because our default forward is different
        alpha += Mathf.PI / 2.0f;

        // Determine our theta with the dot product, include offset
        theta = -Mathf.Acos(Vector3.Dot(thetaVector, Vector3.up)) + Mathf.PI / 2.0f;

        // Start transition routine
        StartCoroutine(TransitionCameraFromTarget(target));
        target = null;
    }


    // Use LateUpdate to avoid camera jitter
    void LateUpdate() {

        if(target != null) {
            TargetedCameraUpdate();
        } else {

            UntargetedCameraUpdate();
        }
    }

    /*
     * Summary: 
     *      Updates camera position and camera lookAt for targeted modde
     */
    void TargetedCameraUpdate() {

        // Mouse Input (also set up for xbox right analog stick)
        Vector2 mouseDelta =  new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));


        // Update camera positions
        horizontalOffset -= mouseDelta.x * Time.deltaTime * mouseSensitivity;

        // Support for inverted controls
        if(inverted) {
            verticalOffset += mouseDelta.y * Time.deltaTime * mouseSensitivity;
        } else {
            verticalOffset -= mouseDelta.y * Time.deltaTime * mouseSensitivity;
        }

        ClampCameraPosition();
        
        // Get camera locations
        Vector3 camTarget = CameraLookatLocationTargeted(target);
        Vector3 camLocation = CameraLocationTargeted(target);

        // Don't set position if transition is updating position
        if(!transitioning) {
            transform.position = camLocation;
            transform.LookAt(camTarget);
        }

        // Update position so nothing obscures the cameras view of its LookAt
        RaycastHit hit;
        if (Physics.Raycast(camTarget, transform.TransformDirection(-Vector3.forward), out hit, (camTarget - camLocation).magnitude, cameraCollisionLayerMask))
        {
            transform.position = hit.point + transform.forward * rayCastOffset;
        }
    }

    /*
     * Summary: 
     *      Updates camera position and LookAt for thirdperson mode
     */
    void UntargetedCameraUpdate() {
        
        // Mouse Input (also set up for xbox right analog stick)
        Vector2 mouseDelta =  new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        
        // Update camera positions
        alpha -= mouseDelta.x * Time.deltaTime * mouseSensitivity;

        // Support for inverted controls
        if(inverted) {
            theta += mouseDelta.y * Time.deltaTime * mouseSensitivity;
        } else {
            theta -= mouseDelta.y * Time.deltaTime * mouseSensitivity;
        }

        ClampCameraPosition();

        // Get camera locations
        Vector3 camLocation = CameraLocationUntargeted();
        if(!transitioning) {
            transform.position = camLocation;
            transform.LookAt(player);
        }

        // Update position so nothing obscures the cameras view of its LookAt
        RaycastHit hit;
        if (Physics.Raycast(player.position, transform.TransformDirection(-Vector3.forward), out hit, (player.position - camLocation).magnitude, cameraCollisionLayerMask))
        {
            transform.position = hit.point + transform.forward * rayCastOffset;
        }
    }

    /*
     * Summary: 
     *      Clamps camera positions to prevent unwanted camera flipping
     */
    void ClampCameraPosition() {

        // Clamp alpha and theta to prevent camera flipping for thirdperson mode
        if(theta > Mathf.PI / 2.0f - .1f) {
            theta = Mathf.PI / 2.0f - .1f;
        }

        if(theta < -Mathf.PI / 2.0f + .1f) {
            theta = -Mathf.PI / 2.0f + .1f;
        }

        if(alpha > Mathf.PI * 2.0f) {
            alpha -= Mathf.PI * 2.0f;
        }

        if(alpha < 0) {
            alpha += Mathf.PI * 2.0f;
        }

        // Clamp offets to be within set bounds for targeted mode
        if(verticalOffset > maxVerticalOffset) {
            verticalOffset = maxVerticalOffset;
        }

        if(verticalOffset < minVerticalOffset) {
            verticalOffset = minVerticalOffset;
        }

        if(horizontalOffset > maxHorizontalOffset) {
            horizontalOffset = maxHorizontalOffset;
        }

        if(horizontalOffset < minHorizontalOffset) {
            horizontalOffset = minHorizontalOffset;
        }
    }

    /*
     * Summary: 
     *      Calculates position of camera for thirdperson mode
     * Returns:
     *      The position in worldspace of the camera
     */
    Vector3 CameraLocationUntargeted() {
        
        // Calculate camera position for thirdperson camera
        return player.transform.position + new Vector3(Mathf.Cos(theta) * Mathf.Cos(alpha), Mathf.Sin(theta), Mathf.Cos(theta) * Mathf.Sin(alpha)) * cameraDistance;
    }


    /*
     * Summary: 
     *      Calculates position of camera for targeted mode
     * Parameters:
     *      camTarget:
     *          The target to use in calculations for camera position
     * Returns:
     *      The position in worldspace of the camera 
     */
    Vector3 CameraLocationTargeted(Transform camTarget) {

        // Get initial target vector
        Vector3 targetVector = camTarget.transform.position - player.transform.position;

        // position camera behind the player based on the targets position and scale it based on distance from target, clamp if we're more than our max distance
        Vector3 location = player.transform.position - targetVector.normalized * cameraDistance * Mathf.Max(targetVector.magnitude / distanceScalar, 1.0f);

        // Set camera location based on offsets
        location += horizontalOffset * player.transform.right + verticalOffset * player.transform.up;
        return location;
    }

    /*
     * Summary: 
     *      Calculates the LookAt of camera for targeted mode
     * Parameters:
     *      camTarget:
     *          The target to use in calculations for camera LookAt
     * Returns:
     *      The LookAt of the camera in worldspace
     */
    Vector3 CameraLookatLocationTargeted(Transform camTarget){

        // Intialize target vector
        Vector3 targetVector = camTarget.transform.position - player.transform.position;
        // Look at the middle point between player and target
        return player.transform.position + targetVector * .5f;
    }


    /*
     * Summary: 
     *      Coroutine to transition the camera to targeted mode smoothly
     * Parameters:
     *      transitionTarget:
     *          Transform of target for targeted mode
     * Returns:
     *      IEnumerator for coroutine
     */
    IEnumerator TransitionCameraToTarget(Transform transitionTarget) {

        // Start transition
        transitioning = true;

        // Inital values for camera
        Vector3 camLocationTargeted = CameraLocationTargeted(transitionTarget);
        Vector3 camTarget = CameraLookatLocationTargeted(transitionTarget);
        Vector3 camLocationUntargeted =  CameraLocationUntargeted();
        
        // Initialize counter for coroutine
        float counter = 0;
        
        // Return for one frame for timing
        yield return null;
        // While not reached transition time, interpolate camera position
        while (counter < camTransitionTime) {
            // Add frame time to our counter
            counter += Time.deltaTime;

            // Recalculate current camera positions
            camLocationTargeted = CameraLocationTargeted(transitionTarget);
            camTarget = CameraLookatLocationTargeted(transitionTarget);

            // Interpolate between camera positions over time
            transform.position = Vector3.Lerp(camLocationUntargeted, camLocationTargeted, Mathf.Min(counter/camTransitionTime, 1.0f));
            // Interpolate between camera lookats over time
            transform.LookAt(Vector3.Lerp(player.transform.position, camTarget, Mathf.Min(counter/camTransitionTime, 1.0f)));
            // Wait until next frame
            yield return null;
        }
        // Stop transtion
        transitioning = false;
    }

    /*
     * Summary: 
     *      Coroutine to transition the camera to thirdperson mode smoothly
     * Parameters:
     *      transitionTarget:
     *          Transform of target the camera was using for targeted mode
     * Returns:
     *      IEnumerator for coroutine
     */
    IEnumerator TransitionCameraFromTarget(Transform transitionTarget) {

        // Start transition
        transitioning = true;

        // Inital values for camera
        Vector3 camLocationTargeted = CameraLocationTargeted(transitionTarget);
        Vector3 camTarget = CameraLookatLocationTargeted(transitionTarget);
        Vector3 camLocationUntargeted =  CameraLocationUntargeted();

        // Initialize counter for coroutine
        float counter = 0;

        // Return for one frame for timing
        yield return null;
        // While not reached transition time, interpolate camera position
        while (counter < camTransitionTime) {
            // Add frame time to our counter
            counter += Time.deltaTime;
            
            // Recalculate current camera positions
            camLocationUntargeted =  CameraLocationUntargeted();

            // Interpolate between camera positions over time
            transform.position = Vector3.Lerp(camLocationTargeted, camLocationUntargeted, Mathf.Min(counter/camTransitionTime, 1.0f));
            // Interpolate between camera lookats over time
            transform.LookAt(Vector3.Lerp(camTarget, player.transform.position, Mathf.Min(counter/camTransitionTime, 1.0f)));
            // Wait until next frame
            yield return null;
        }
        // Stop transtion
        transitioning = false;
    }
}
