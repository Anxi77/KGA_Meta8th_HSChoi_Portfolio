using UnityEngine;

public class Portal : MonoBehaviour
{
    private StageManager.SceneType destinationType;

    public void Initialize(StageManager.SceneType destType)
    {
        destinationType = destType;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StageManager.Instance.OnPortalEnter(destinationType);
        }
    }
}