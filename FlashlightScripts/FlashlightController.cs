using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Light spotLight;
    [SerializeField] private Camera playerCamera;

    [Header("Battery Settings")]
    public float batteryHealth = 0;
    public int batteryCount = 0;

    [SerializeField] private float maxBatteryHealth = 60f;

    [Header("Stun Settings")]
    [SerializeField] private float stunRange = 10f;
    [SerializeField] private float stunDrainMultiplier = 4f;

    private bool isOpen = false;
    public bool isLightOn => isOpen;

    private bool isBlinking = false;
    private float blinkTimer = 0f;
    [SerializeField] private float blinkInterval = 0.2f;

    private Transform playerHand;
    private bool hasNotifiedEmpty = false;

    // Bildirim Spam Engelleyici
    private float notificationCooldown = 0f;

    void Start()
    {
        InitializeReferences();
        UpdateLightState();
    }

    public void SetupFlashlight(Transform handRef, Camera camRef)
    {
        playerHand = handRef;
        playerCamera = camRef;
        if (playerCamera == null) playerCamera = Camera.main;
        UpdateLightState();
    }

    void OnEnable() { InitializeReferences(); }

    public void InitializeReferences()
    {
        if (playerCamera != null && playerHand != null) return;
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerHand == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerHand = player.transform.Find("Hand");
        }
        UpdateLightState();
    }

    private void UpdateLightState()
    {
        if (spotLight != null) spotLight.enabled = isOpen;
    }

    void Update()
    {
        if (playerHand == null) { InitializeReferences(); return; }
        if (transform.parent != playerHand) return;

        // Cooldown süresini her saniye azalt
        if (notificationCooldown > 0)
        {
            notificationCooldown -= Time.deltaTime;
        }

        // --- F TUÞU ---
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (batteryHealth > 0)
            {
                ToggleLight();
                if (VoiceManager.Instance != null) VoiceManager.Instance.PlayFlashlightToggle();
            }
            else
            {
                if (VoiceManager.Instance != null) VoiceManager.Instance.PlayFlashlightToggle();

                if (isOpen) ToggleLightOff();

                // Bildirim Mantýðý
                if (notificationCooldown <= 0)
                {
                    if (NotificationManager.Instance != null)
                    {
                        if (batteryCount > 0)
                            NotificationManager.Instance.ShowNotification(NotificationType.Uyari,
                               "Pili Deðiþtirmek için [R]", KeyCode.R);
                        else
                            NotificationManager.Instance.ShowNotification(NotificationType.Warning,
                                "Pil Bulunmuyor!");
                    }
                    notificationCooldown = 2.0f;
                }
            }
        }

        // R Tuþu
        if (Input.GetKeyDown(KeyCode.R)) ReloadBattery();

        // Fener Açýk ve Pil Var
        if (isOpen && batteryHealth > 0)
        {
            float currentDrain = 1f;
            if (playerCamera != null)
            {
                RaycastHit hit;
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, stunRange))
                {
                    EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
                    if (enemy != null) { enemy.TakeStun(); currentDrain = stunDrainMultiplier; }
                }
            }

            batteryHealth -= Time.deltaTime * currentDrain;

            // Pil azaldýysa (Son 2 saniye) blink baþlasýn
            if (batteryHealth <= 2 && !isBlinking)
            {
                isBlinking = true;
            }
        }

        // --- BLINK VE GLITCH SESÝ MANTIÐI ---
        if (isBlinking && isOpen && batteryHealth > 0)
        {
            blinkTimer += Time.deltaTime;
            if (blinkTimer >= blinkInterval)
            {
                spotLight.enabled = !spotLight.enabled;
                blinkTimer = 0f;

                // Iþýk her söndüðünde glitch sesi çalmaya çalýþ
                if (!spotLight.enabled && VoiceManager.Instance != null)
                {
                    // 20 Saniyelik ses olduðu için VoiceManager içinde kontrol ediyoruz,
                    // eðer zaten çalýyorsa baþtan baþlatmayacak.
                    if (batteryHealth > 0.05f)
                    {
                        VoiceManager.Instance.PlayFlashlightGlitch();
                    }
                }
            }
        }
        else if (isOpen && !isBlinking && batteryHealth > 0)
        {
            if (spotLight != null && !spotLight.enabled) spotLight.enabled = true;
        }

        // --- PÝL BÝTTÝÐÝ AN ---
        if (batteryHealth <= 0 && !hasNotifiedEmpty)
        {
            if (isOpen)
            {
                ToggleLightOff();
                if (VoiceManager.Instance != null) VoiceManager.Instance.PlayFlashlightToggle();
            }

            batteryHealth = 0;
            isBlinking = false;

            // 1. Iþýðý Kapat
            if (spotLight != null) spotLight.enabled = false;

            // 2. GLITCH SESÝNÝ ZORLA DURDUR (En önemli kýsým burasý)
            if (VoiceManager.Instance != null) VoiceManager.Instance.StopFlashlightGlitch();

            hasNotifiedEmpty = true;

            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification(NotificationType.PilBildirimi,
                    $"Pil Tükendi. (Kalan: {batteryCount})");

                if (batteryCount > 0)
                {
                    NotificationManager.Instance.ShowNotification(NotificationType.PilBildirimi,
                        "Pili Deðiþtirmek için [R]", KeyCode.R);
                }
            }
        }
    }

    private void ToggleLight()
    {
        isOpen = !isOpen;
        UpdateLightState();
        isBlinking = false;
        blinkTimer = 0f;

        // Eðer feneri elle kapatýrsak da glitch sesini keselim
        if (!isOpen && VoiceManager.Instance != null)
        {
            VoiceManager.Instance.StopFlashlightGlitch();
        }
    }

    public void ToggleLightOff()
    {
        isOpen = false;
        UpdateLightState();

        // Zorla kapatýldýðýnda da sesi kes
        if (VoiceManager.Instance != null)
        {
            VoiceManager.Instance.StopFlashlightGlitch();
        }
    }

    private void ReloadBattery()
    {
        if (batteryCount > 0 && batteryHealth < maxBatteryHealth)
        {
            // Pili yenilerken de eski cýzýrtýyý keselim
            if (VoiceManager.Instance != null) VoiceManager.Instance.StopFlashlightGlitch();

            batteryHealth = maxBatteryHealth;
            batteryCount--;
            hasNotifiedEmpty = false;

            if (VoiceManager.Instance != null) VoiceManager.Instance.PlayFlashlightReload();

            Debug.Log($"Pil Yenilendi. Kalan Pil: {batteryCount}");
        }
        else if (batteryCount <= 0)
        {
            if (NotificationManager.Instance != null && notificationCooldown <= 0)
            {
                NotificationManager.Instance.ShowNotification(NotificationType.Warning, "Pil Bulunmuyor!");
                notificationCooldown = 2.0f;
            }
        }
    }

    public void IncreaseBatteryCount() { batteryCount++; }
}