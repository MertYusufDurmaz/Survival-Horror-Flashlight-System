using UnityEngine;

public class FlashlightCollectable : MonoBehaviour, ICollectable, ITargetable
{
    [SerializeField] private FlashlightController flashlightController;
    [SerializeField] private ItemData itemData;

    public ItemData ItemDataProperty => itemData;

    void Awake()
    {
        if (flashlightController == null)
        {
            flashlightController = GetComponent<FlashlightController>();
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    public void Collect(Transform collectorHand)
    {
        // --- BU KISIM EKSÝKTÝ, ÞÝMDÝ EKLENDÝ ---
        // Fener alýndýðýnda ID'sini SaveManager'a bildir
        UniqueID uid = GetComponent<UniqueID>();
        if (uid != null)
        {
            if (GameSaveManager.Instance != null)
            {
                GameSaveManager.Instance.MarkObjectAsCollected(uid.uniqueID);
                Debug.Log($"Fener Toplandý ve Kaydedildi! ID: {uid.uniqueID}");
            }
        }
        else
        {
            Debug.LogWarning("Fener üzerinde UniqueID scripti bulunamadý! Kayýt yapýlamaz.");
        }
        // ---------------------------------------

        transform.SetParent(collectorHand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Ölçeði düzelt (Cýlýz ýþýk sorunu için garanti)
        transform.localScale = Vector3.one;

        // Objenin kendisini aktif tut (Scriptler çalýþsýn)
        gameObject.SetActive(true);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        if (flashlightController != null)
        {
            flashlightController.enabled = true;
            // El referansýný ve kamerayý verelim
            flashlightController.SetupFlashlight(collectorHand, Camera.main);
        }

        // Toplama iþlemi bittiði için bu scripti devre dýþý býrakabiliriz
        // ama objeyi kapatmýyoruz.
        this.enabled = false;
    }

    public void Drop(Vector3 dropPosition, Quaternion dropRotation)
    {
        transform.SetParent(null);
        transform.position = dropPosition;
        transform.rotation = dropRotation;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(Camera.main.transform.forward * 2f, ForceMode.Impulse);
        }

        if (flashlightController != null)
        {
            flashlightController.enabled = false;
        }

        this.enabled = true;
    }

    public void AddToInventory()
    {
        if (itemData != null && InventoryManager.Instance != null)
        {
            // Envantere eklemeden önce de ID'yi bildirmeliyiz (Güvenlik için)
            UniqueID uid = GetComponent<UniqueID>();
            if (uid != null && GameSaveManager.Instance != null)
            {
                GameSaveManager.Instance.MarkObjectAsCollected(uid.uniqueID);
            }

            InventoryManager.Instance.AddItem(itemData, this);

            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.CompleteTask("task_find_flashlight");
                Debug.Log("Fener görevi tamamlandý.");
            }
        }
        else
        {
            Debug.LogWarning("Flashlight'ýn ItemData'sý veya InventoryManager eksik.");
        }
    }

    public void ToggleHighlight(bool highlight)
    {
        // Debug.Log(gameObject.name + " Highlight: " + highlight);
    }
}