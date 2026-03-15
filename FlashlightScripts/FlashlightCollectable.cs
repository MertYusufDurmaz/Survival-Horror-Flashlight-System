using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class FlashlightCollectable : MonoBehaviour, ICollectable, ITargetable
{
    [Header("References")]
    [SerializeField] private FlashlightController flashlightController;
    [SerializeField] private ItemData itemData;

    [Header("Events")]
    [Tooltip("Fener toplandığında çalışacak olaylar (Save işlemi, Görev tamamlama, Envantere ekleme)")]
    public UnityEvent onFlashlightCollected;

    public ItemData ItemDataProperty => itemData;

    void Awake()
    {
        if (flashlightController == null)
            flashlightController = GetComponent<FlashlightController>();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
    }

    public void Collect(Transform collectorHand)
    {
        // Tüm dış bağımlılıkları (Save, Task, Inventory) bu Event üzerinden yöneteceğiz.
        // Inspector'dan yöneticilerinizi bu event'e bağlayabilirsiniz.
        onFlashlightCollected?.Invoke();

        transform.SetParent(collectorHand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        gameObject.SetActive(true);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        if (flashlightController != null)
        {
            flashlightController.enabled = true;
            flashlightController.SetupFlashlight(collectorHand, Camera.main);
        }

        this.enabled = false; // Toplanma scriptini kapat
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

        this.enabled = true; // Tekrar toplanabilmesi için aç
    }

    public void AddToInventory()
    {
        // Eğer envanter sisteminiz bu metodu özel olarak çağırıyorsa, 
        // içeriğini Event'ler üzerinden yürütecek şekilde silebilir 
        // veya doğrudan projenizdeki InventoryManager'a entegre bırakabilirsiniz.
        if (itemData != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemData, this);
        }
    }

    public void ToggleHighlight(bool highlight)
    {
        // Shader highlight işlemleri buraya eklenebilir.
    }
}
