using UnityEngine;
using UnityEngine.Events;

public class FlashlightController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Light spotLight;
    [SerializeField] private Camera playerCamera;

    [Header("Controls")]
    public KeyCode toggleKey = KeyCode.F;
    public KeyCode reloadKey = KeyCode.R;

    [Header("Battery Settings")]
    public float batteryHealth = 0;
    public int batteryCount = 0;
    [SerializeField] private float maxBatteryHealth = 60f;

    [Header("Stun Settings")]
    [SerializeField] private float stunRange = 10f;
    [SerializeField] private float stunDrainMultiplier = 4f;
    [Tooltip("Düşmanı tespit etmek için kullanılacak katman")]
    [SerializeField] private LayerMask enemyLayerMask;

    [Header("Events (Audio & UI)")]
    public UnityEvent onFlashlightToggle;
    public UnityEvent onFlashlightReload;
    public UnityEvent onFlashlightGlitch;
    public UnityEvent onGlitchStop;
    public UnityEvent onBatteryEmpty;
    public UnityEvent<string> onNotificationTriggered;

    private bool isOpen = false;
    public bool isLightOn => isOpen;

    private bool isBlinking = false;
    private float blinkTimer = 0f;
    [SerializeField] private float blinkInterval = 0.2f;

    private Transform playerHand;
    private bool hasNotifiedEmpty = false;
    private float notificationCooldown = 0f;

    void Start()
    {
        InitializeReferences();
        UpdateLightState();
    }

    public void SetupFlashlight(Transform handRef, Camera camRef)
    {
        playerHand = handRef;
        playerCamera = camRef != null ? camRef : Camera.main;
        UpdateLightState();
    }

    public void InitializeReferences()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerHand == null && transform.parent != null)
        {
            playerHand = transform.parent;
        }
        UpdateLightState();
    }

    private void UpdateLightState()
    {
        if (spotLight != null) spotLight.enabled = isOpen;
    }

    void Update()
    {
        if (playerHand == null || transform.parent != playerHand) return;

        if (notificationCooldown > 0) notificationCooldown -= Time.deltaTime;

        HandleInputs();
        HandleBatteryAndStun();
        HandleBlinking();
    }

    private void HandleInputs()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (batteryHealth > 0)
            {
                ToggleLight();
                onFlashlightToggle?.Invoke();
            }
            else
            {
                onFlashlightToggle?.Invoke();
                if (isOpen) ToggleLightOff();

                if (notificationCooldown <= 0)
                {
                    string msg = batteryCount > 0 ? "Pili Değiştirmek için [" + reloadKey.ToString() + "]" : "Pil Bulunmuyor!";
                    onNotificationTriggered?.Invoke(msg);
                    notificationCooldown = 2.0f;
                }
            }
        }

        if (Input.GetKeyDown(reloadKey)) ReloadBattery();
    }

    private void HandleBatteryAndStun()
    {
        if (isOpen && batteryHealth > 0)
        {
            float currentDrain = 1f;
            
            // Düşman sersemletme (Raycast)
            if (playerCamera != null)
            {
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, stunRange, enemyLayerMask))
                {
                    EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
                    if (enemy != null) 
                    { 
                        enemy.TakeStun(); 
                        currentDrain = stunDrainMultiplier; 
                    }
                }
            }

            batteryHealth -= Time.deltaTime * currentDrain;

            if (batteryHealth <= 2 && !isBlinking)
            {
                isBlinking = true;
            }

            // Pil bitti
            if (batteryHealth <= 0 && !hasNotifiedEmpty)
            {
                HandleEmptyBattery();
            }
        }
    }

    private void HandleBlinking()
    {
        if (isBlinking && isOpen && batteryHealth > 0)
        {
            blinkTimer += Time.deltaTime;
            if (blinkTimer >= blinkInterval)
            {
                spotLight.enabled = !spotLight.enabled;
                blinkTimer = 0f;

                if (!spotLight.enabled && batteryHealth > 0.05f)
                {
                    onFlashlightGlitch?.Invoke();
                }
            }
        }
        else if (isOpen && !isBlinking && batteryHealth > 0)
        {
            if (spotLight != null && !spotLight.enabled) spotLight.enabled = true;
        }
    }

    private void HandleEmptyBattery()
    {
        if (isOpen)
        {
            ToggleLightOff();
            onFlashlightToggle?.Invoke();
        }

        batteryHealth = 0;
        isBlinking = false;
        if (spotLight != null) spotLight.enabled = false;

        onGlitchStop?.Invoke();
        onBatteryEmpty?.Invoke();
        hasNotifiedEmpty = true;

        onNotificationTriggered?.Invoke($"Pil Tükendi. (Kalan: {batteryCount})");
    }

    private void ToggleLight()
    {
        isOpen = !isOpen;
        UpdateLightState();
        isBlinking = false;
        blinkTimer = 0f;

        if (!isOpen) onGlitchStop?.Invoke();
    }

    public void ToggleLightOff()
    {
        isOpen = false;
        UpdateLightState();
        onGlitchStop?.Invoke();
    }

    private void ReloadBattery()
    {
        if (batteryCount > 0 && batteryHealth < maxBatteryHealth)
        {
            onGlitchStop?.Invoke();
            batteryHealth = maxBatteryHealth;
            batteryCount--;
            hasNotifiedEmpty = false;
            
            onFlashlightReload?.Invoke();
        }
        else if (batteryCount <= 0 && notificationCooldown <= 0)
        {
            onNotificationTriggered?.Invoke("Pil Bulunmuyor!");
            notificationCooldown = 2.0f;
        }
    }

    public void IncreaseBatteryCount() { batteryCount++; }
}
