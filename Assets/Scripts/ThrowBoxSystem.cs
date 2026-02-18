using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowBoxSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private BoxCollider slideArea;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip throwSound;
    [SerializeField] private LayerMask boxLayer;

    [Header("Settings")]
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private float moveSmoothing = 15f;

    private Camera cam;
    private GameObject currentBox;
    private Rigidbody currentBoxRb;
    private CubeMergeSystem currentBoxMerge;
    
    private Vector2 pointerPos;
    private bool isPressing;
    private float spawnTimer;
    private float screenDepth;

    private void Awake()
    {
        cam = Camera.main;
        
        if (spawnPoint == null) spawnPoint = transform;
        
        screenDepth = Vector3.Distance(cam.transform.position, spawnPoint.position);

        if (!boxPrefab || !slideArea || !audioSource)
            Debug.LogError("ThrowBoxSystem: Missing essential references!", this);
    }

    private void Update()
    {
        HandleSpawning();
        HandleBoxMovement();
    }

    private void HandleSpawning()
    {
        if (currentBox == null)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval && !IsAreaOccupied())
            {
                SpawnBox();
                spawnTimer = 0;
            }
        }
    }

    private void SpawnBox()
    {
        currentBox = Instantiate(boxPrefab, spawnPoint.position, boxPrefab.transform.rotation);
        
        currentBoxRb = currentBox.GetComponent<Rigidbody>();
        currentBoxMerge = currentBox.GetComponent<CubeMergeSystem>();

        if (currentBoxRb) currentBoxRb.isKinematic = true;
        
        if (currentBoxMerge)
        { 
            currentBoxMerge.SetNewCube(true);
            currentBoxMerge.SetupInitialLevel();
        }
    }

    private void HandleBoxMovement()
    {
        if (!isPressing || currentBox == null) return;

        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(pointerPos.x, pointerPos.y, screenDepth));
        
        float clampedX = Mathf.Clamp(worldPos.x, slideArea.bounds.min.x, slideArea.bounds.max.x);

        Vector3 targetPosition = new Vector3(
            clampedX,
            currentBox.transform.position.y,
            currentBox.transform.position.z
        );

        currentBox.transform.position = Vector3.Lerp(
            currentBox.transform.position, 
            targetPosition, 
            Time.deltaTime * moveSmoothing
        );
    }

    private bool IsAreaOccupied()
    {
        Vector3 center = slideArea.transform.TransformPoint(slideArea.center);
        Vector3 halfExtents = Vector3.Scale(slideArea.size, slideArea.transform.lossyScale) * 0.5f;
        Collider[] colliders = Physics.OverlapBox(center, halfExtents, slideArea.transform.rotation, boxLayer);

        return colliders.Length > 0;
    }

    public void BoxThrow()
    {
        if (currentBox == null) return;

        if (currentBoxRb)
        {
            currentBoxRb.isKinematic = false;
            currentBoxRb.AddForce(Vector3.forward * throwForce, ForceMode.Impulse);
        }

        if (currentBoxMerge) currentBoxMerge.SetNewCube(false);

        if (audioSource && throwSound)
            audioSource.PlayOneShot(throwSound);

        currentBox = null;
        currentBoxRb = null;
        currentBoxMerge = null;
    }

    public void OnPointerPosition(InputAction.CallbackContext context)
    {
        pointerPos = context.ReadValue<Vector2>();
    }

    public void OnPress(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isPressing = true;
        }
        else if (context.canceled)
        {
            isPressing = false;
            BoxThrow();
        }
    }
}