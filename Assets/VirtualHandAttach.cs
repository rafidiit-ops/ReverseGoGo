using UnityEngine;
using UnityEngine.InputSystem;

public class VirtualHandAttach : MonoBehaviour
{
    public Transform virtualHand;                 // The ghost hand visual
    public Transform controllerTransform;         // The actual right-hand controller
    public RaycastObjectSelector selector;        // The raycast script
    public InputActionProperty triggerAction;     // XRI RightHand Activate (attach hand)
    public InputActionProperty gripAction;        // XRI RightHand Grip (remote pull)
    public HandCalibrationDepthScale depthScale;  // Hand calibration system

    private bool isAttached = false;              // Trigger mode (hand attached)
    private bool isRemotePulling = false;         // Grip mode (remote pull with scaling)
    private GameObject currentlyGrabbedObject;
    private float handOffset = 0.6f;
    
    private Vector3 controllerStartPos;           // Controller position when grab started
    private Vector3 cubeStartPos;                 // Cube position when grab started
    private Vector3 controllerPullStartPos;       // Controller position when grip started (for exponential pull calculation)
    private float initialDistanceToController;   // Initial distance from cube to controller when grip started

    // Public accessors for UserStudyManager
    public bool IsAttached() { return isAttached; }
    public bool IsRemotePulling() { return isRemotePulling; }
    public GameObject GetCurrentObject() { return currentlyGrabbedObject; }
    public RaycastObjectSelector GetSelector() { return selector; }

    void Start()
    {
        if (virtualHand == null)
        {
            Debug.LogError("VirtualHandAttach: virtualHand is not assigned!");
        }

        if (controllerTransform == null)
        {
            Debug.LogError("VirtualHandAttach: controllerTransform is not assigned!");
        }

        if (selector == null)
        {
            Debug.LogError("VirtualHandAttach: selector (RaycastObjectSelector) is not assigned!");
        }
        
        // Enable input actions
        triggerAction.action.Enable();
        gripAction.action.Enable();
    }

    void Update()
    {
        if (virtualHand == null || selector == null)
            return;

        // ===== TRIGGER BUTTON: Attach hand to object =====
        if (triggerAction.action.WasPressedThisFrame() && !isAttached && !isRemotePulling)
        {
            GameObject selected = selector.GetCurrentTarget();

            if (selected != null)
            {
                AttachHand(selected);
            }
        }

        // Release on trigger release
        if (isAttached && triggerAction.action.WasReleasedThisFrame())
        {
            ReleaseHand();
        }

        // ===== GRIP BUTTON: Remote pull with exponential scaling =====
        if (gripAction.action.WasPressedThisFrame() && !isRemotePulling && !isAttached)
        {
            GameObject selected = selector.GetCurrentTarget();

            if (selected != null)
            {
                StartRemotePull(selected);
            }
        }

        // Release on grip release
        if (isRemotePulling && gripAction.action.WasReleasedThisFrame())
        {
            StopRemotePull();
        }

        // Apply hand attachment movement
        if (isAttached && currentlyGrabbedObject != null)
        {
            ApplyHandAttachmentMovement();
        }

        // Apply remote pull movement
        if (isRemotePulling && currentlyGrabbedObject != null)
        {
            ApplyRemotePullMovement();
        }
    }

    private void ApplyHandAttachmentMovement()
    {
        // Calculate how much controller has moved (using actual controller, not virtual hand)
        Vector3 controllerDelta = controllerTransform.position - controllerStartPos;
        
        // Calculate target position (follow controller movement; positive mapping)
        Vector3 targetPos = cubeStartPos + controllerDelta;
        
        // Use Rigidbody for physics-based movement if available (and not kinematic)
        Rigidbody rb = currentlyGrabbedObject.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            // Calculate velocity needed to reach target position
            Vector3 positionDiff = targetPos - currentlyGrabbedObject.transform.position;
            rb.linearVelocity = positionDiff / Time.deltaTime;
            
            // Prevent rotation from physics
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            // Fallback: direct position if no Rigidbody or if kinematic
            currentlyGrabbedObject.transform.position = targetPos;
        }
        
        // Keep virtual hand visible and positioned with object during trigger mode
        Vector3 cubePos = currentlyGrabbedObject.transform.position;
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 dirToCamera = (cameraPos - cubePos).normalized;
        virtualHand.position = cubePos + dirToCamera * handOffset;
        virtualHand.LookAt(cubePos);
    }

    private void ApplyRemotePullMovement()
    {
        if (depthScale == null)
            return;

        // Calculate how much the controller has retracted from start position
        float controllerRetractionDistance = Vector3.Distance(controllerPullStartPos, controllerTransform.position);
        
        // Calculate total pull range (from full extension to threshold)
        float maxPullDistance = depthScale.GetDistanceBeyondThreshold(); // Distance beyond threshold at start
        
        // Normalize retraction: 0 = just started, 1 = reached threshold
        float retractionProgress = Mathf.Clamp01(controllerRetractionDistance / maxPullDistance);
        
        // Calculate current distance from object to controller
        float currentDistanceToController = Vector3.Distance(currentlyGrabbedObject.transform.position, controllerTransform.position);
        
        // Distance-based exponential speed: farther = faster, closer = slower
        // Normalize distance: 0 = at hand, 1 = at initial distance
        float normalizedDistance = Mathf.Clamp01(currentDistanceToController / initialDistanceToController);
        
        // Exponential speed multiplier based on distance (inverted)
        // When far (normalizedDistance = 1): high speed multiplier
        // When close (normalizedDistance = 0): low speed multiplier
        float distanceSpeedFactor = Mathf.Pow(normalizedDistance, 1.0f / depthScale.exponentialPower) * depthScale.maxScalingFactor;
        
        // Clamp to reasonable range (minimum 0.1x to prevent stopping completely)
        distanceSpeedFactor = Mathf.Max(0.1f, distanceSpeedFactor);
        
        // Calculate exponential progress based on retraction
        float exponentialProgress = Mathf.Pow(retractionProgress, depthScale.exponentialPower);
        
        // Calculate target position: interpolate from start position toward controller
        Vector3 targetPos = Vector3.Lerp(cubeStartPos, controllerTransform.position, exponentialProgress);
        
        // Move object toward target using physics with distance-based speed
        Rigidbody rb = currentlyGrabbedObject.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            // Calculate velocity with distance-based speed factor
            Vector3 positionDiff = targetPos - currentlyGrabbedObject.transform.position;
            rb.linearVelocity = (positionDiff / Time.deltaTime) * distanceSpeedFactor;
            
            // Prevent rotation from physics
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            // Fallback: direct position if no Rigidbody or if kinematic
            currentlyGrabbedObject.transform.position = targetPos;
        }
        
        // Check if object reached the controller (within 0.15m threshold)
        if (currentDistanceToController < 0.15f)
        {
            // Move virtual hand far away (off-screen) when object reaches controller
            if (virtualHand != null)
            {
                virtualHand.position = new Vector3(1000f, 1000f, 1000f);
            }
            
            // Automatically switch to hand attachment mode when object reaches controller
            Debug.Log("✅ Object reached controller - switching to hand attachment mode");
            
            // Stop remote pull and start hand attachment
            isRemotePulling = false;
            isAttached = true;
            
            // Store new starting positions for hand attachment
            controllerStartPos = controllerTransform.position;
            cubeStartPos = currentlyGrabbedObject.transform.position;
        }
        else
        {
            // Keep virtual hand aligned with object during remote pull
            Vector3 cubePos = currentlyGrabbedObject.transform.position;
            Vector3 cameraPos = Camera.main.transform.position;
            Vector3 dirToCamera = (cameraPos - cubePos).normalized;
            virtualHand.position = cubePos + dirToCamera * handOffset;
            virtualHand.LookAt(cubePos);
        }
        
        // Debug log every 30 frames
        if (Time.frameCount % 30 == 0)
        {
            Debug.Log($"🎯 Exponential Pull | Progress: {exponentialProgress:F2} | Distance: {currentDistanceToController:F3}m | Speed: {distanceSpeedFactor:F2}x");
        }
    }

    private void AttachHand(GameObject objectToGrab)
    {
        currentlyGrabbedObject = objectToGrab;
        isAttached = true;

        // Notify selector that object is grabbed (disables highlighting)
        if (selector != null)
        {
            selector.SetGrabbedState(true);
        }

        // Get cube size for offset
        Renderer renderer = objectToGrab.GetComponent<Renderer>();
        handOffset = 0.6f;
        if (renderer != null)
        {
            handOffset = renderer.bounds.extents.magnitude * 1.2f;
        }

        // Store starting positions for ReverseGoGo calculation
        controllerStartPos = controllerTransform.position;
        cubeStartPos = objectToGrab.transform.position;

        // Position hand towards camera from cube
        Vector3 cubePos = objectToGrab.transform.position;
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 dirToCamera = (cameraPos - cubePos).normalized;
        
        virtualHand.position = cubePos + dirToCamera * handOffset;
        virtualHand.LookAt(cubePos);

        Debug.Log("✋ [TRIGGER] Attached hand to: " + objectToGrab.name);
    }

    private void ReleaseHand()
    {
        if (currentlyGrabbedObject != null)
        {
            // Notify selector that object is released (re-enables highlighting)
            if (selector != null)
            {
                selector.SetGrabbedState(false);
            }
            
            // Restore normal physics when released
            Rigidbody rb = currentlyGrabbedObject.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.constraints = RigidbodyConstraints.None;
            }
            
            Debug.Log("🔓 Released: " + currentlyGrabbedObject.name);
            currentlyGrabbedObject = null;
        }

        // Move virtual hand off-screen when released
        if (virtualHand != null)
        {
            virtualHand.position = new Vector3(1000f, 1000f, 1000f);
        }

        isAttached = false;
    }

    private void StartRemotePull(GameObject objectToPull)
    {
        currentlyGrabbedObject = objectToPull;
        isRemotePulling = true;

        // Notify selector that object is grabbed (disables highlighting)
        if (selector != null)
        {
            selector.SetGrabbedState(true);
        }

        // Store starting positions for exponential pull calculation
        controllerPullStartPos = controllerTransform.position;  // Where pull starts (full extension)
        cubeStartPos = objectToPull.transform.position;          // Cube's starting position
        
        // Calculate initial distance from cube to controller
        initialDistanceToController = Vector3.Distance(cubeStartPos, controllerTransform.position);

        Debug.Log($"🚀 [GRIP] Exponential pull started on: {objectToPull.name}");
        Debug.Log($"   Initial distance to controller: {initialDistanceToController:F2}m");
        Debug.Log($"   Retraction range: 0m to {depthScale.GetDistanceBeyondThreshold():F3}m");
    }

    private void StopRemotePull()
    {
        if (currentlyGrabbedObject != null)
        {
            // Notify selector that object is released (re-enables highlighting)
            if (selector != null)
            {
                selector.SetGrabbedState(false);
            }
            
            // Restore normal physics when released
            Rigidbody rb = currentlyGrabbedObject.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.constraints = RigidbodyConstraints.None;
            }
            
            Debug.Log("🛑 Remote pull stopped: " + currentlyGrabbedObject.name);
            currentlyGrabbedObject = null;
        }

        // Move virtual hand off-screen when pull stops
        if (virtualHand != null)
        {
            virtualHand.position = new Vector3(1000f, 1000f, 1000f);
        }

        isRemotePulling = false;
    }
}
