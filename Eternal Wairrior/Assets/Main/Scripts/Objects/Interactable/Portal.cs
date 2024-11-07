using UnityEngine;

public class Portal : MonoBehaviour
{
    private string destinationScene;
    private System.Action onEnterAction;

    public void Initialize(string scene, System.Action enterAction)
    {
        destinationScene = scene;
        onEnterAction = enterAction;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            onEnterAction?.Invoke();
        }
    }
}