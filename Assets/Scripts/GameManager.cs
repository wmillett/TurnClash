using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private IsometricGroundManager groundManager;
    [SerializeField] private UnitSpawner unitSpawner;
    
    private static GameManager instance;
    
    public static GameManager Instance
    {
        get { return instance; }
    }
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        
        // Ensure managers are found
        if (groundManager == null)
            groundManager = FindObjectOfType<IsometricGroundManager>();
            
        if (unitSpawner == null)
            unitSpawner = FindObjectOfType<UnitSpawner>();
    }
    
    private void Start()
    {
        // The ground manager will create the grid in its Start method
        // The unit spawner will spawn units in its Start method (with a frame delay)
        Debug.Log("Game Manager initialized. Ground and units will be set up automatically.");
    }
    
    public IsometricGroundManager GetGroundManager()
    {
        return groundManager;
    }
    
    public UnitSpawner GetUnitSpawner()
    {
        return unitSpawner;
    }
} 