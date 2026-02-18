using UnityEngine;
using System;

public class CubeMergeSystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private CubeMaterialDatabase materialDatabase;
    [SerializeField] private AudioClip mergeSound;

    public static event Action<int, Vector3> OnCubeMerged;
    public static event Action OnMaxLevelReached;

    private ParticleSystem mergeEffect;
    private AudioSource audioSource;
    private Renderer ren;
    private Rigidbody rb;
    
    private int level;
    private bool isDestroyed = false;
    private bool isNewBox = true;

    public int Level => level;
    public bool IsNewBox => isNewBox;

    private void Awake()
    {
        ren = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        mergeEffect = GetComponentInChildren<ParticleSystem>();
    }

    public void SetupInitialLevel()
    {
        level = (UnityEngine.Random.value <= 0.25f) ? 1 : 0;
        if (materialDatabase != null)
            ren.sharedMaterial = materialDatabase.GetMaterial(level);
    }

    public void SetNewCube(bool isNew)
    {
        isNewBox = isNew;
    }

    private void OnCollisionEnter(Collision collision) => TryProcessMerge(collision);
    private void OnCollisionStay(Collision collision) => TryProcessMerge(collision);

    private void TryProcessMerge(Collision collision)
    {
        if (isDestroyed || isNewBox || !collision.gameObject.CompareTag("Cube")) return;

        if (collision.gameObject.TryGetComponent(out CubeMergeSystem other))
        {
            if (other.level == level && !other.isDestroyed && !other.isNewBox)
            {
                if (GetInstanceID() > other.GetInstanceID())
                    HandleMerge(other);
                else rb.WakeUp();
            }
        }
    }

    private void HandleMerge(CubeMergeSystem other)
    {
        other.isDestroyed = true;

        var main = mergeEffect.main;
        var psr = mergeEffect.GetComponent<ParticleSystemRenderer>();
        if (psr != null) psr.material = materialDatabase.GetMaterial(level);
        
        mergeEffect.Play();

        rb.AddForce(Vector3.up * (level + 3.5f), ForceMode.Impulse);

        OnCubeMerged?.Invoke(level, transform.position);

        ScoreHandler.Instance.AddScore((int)Mathf.Pow(2, level));

        Upgrade();

        if (audioSource && mergeSound)
            audioSource.PlayOneShot(mergeSound);

        if (level >= materialDatabase.MaxLevel)
            OnMaxLevelReached?.Invoke();

        Destroy(other.gameObject);
    }

    private void Upgrade()
    {
        if (level < materialDatabase.MaxLevel)
        {
            level++;
            ren.sharedMaterial = materialDatabase.GetMaterial(level);
            
            rb.WakeUp();
        }
    }
}