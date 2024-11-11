using UnityEngine;
using System.Collections;

public class WorldDropItem : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CircleCollider2D pickupCollider;
    [SerializeField] private float pickupDelay = 0.5f;
    [SerializeField] private float magnetSpeed = 10f;

    private ItemData itemData;
    private bool canPickup = false;
    private Rigidbody2D rb;
    private float dropTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = spriteRenderer ?? GetComponent<SpriteRenderer>();
        pickupCollider = pickupCollider ?? GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        dropTime = Time.time;
        StartCoroutine(EnablePickup());
    }

    private IEnumerator EnablePickup()
    {
        yield return new WaitForSeconds(pickupDelay);
        canPickup = true;
    }

    private void Update()
    {
        if (!canPickup) return;

        var player = GameManager.Instance?.player;
        if (player != null)
        {
            float pickupRange = player.GetComponent<PlayerStat>().GetStat(StatType.ExpCollectionRadius);
            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance <= pickupRange)
            {
                Vector2 direction = (player.transform.position - transform.position).normalized;
                rb.velocity = direction * magnetSpeed;
            }
        }
    }

    public void Initialize(ItemData data)
    {
        itemData = data;
        if (spriteRenderer != null)
        {
            if (data.metadata?.Icon != null)
            {
                spriteRenderer.sprite = data.metadata.Icon;
                Debug.Log($"Set icon from metadata for item: {data.id}");
            }
            else if (data.icon != null)
            {
                spriteRenderer.sprite = data.icon;
                Debug.Log($"Set icon from itemData for item: {data.id}");
            }
            else
            {
                Debug.LogWarning($"No icon found for item: {data.id}");
            }
        }
        else
        {
            Debug.LogError("SpriteRenderer is missing on WorldDropItem!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canPickup) return;
        if (!other.CompareTag("Player")) return;

        var inventory = other.GetComponent<Inventory>();
        if (inventory != null)
        {
            inventory.AddItem(itemData);
            PoolManager.Instance.Despawn<WorldDropItem>(this);
        }
    }
}