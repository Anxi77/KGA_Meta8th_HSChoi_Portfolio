using UnityEngine;
using UnityEngine.UI;

public class SliderManager : MonoBehaviour
{
    private Slider sliderComp;
    public bool noDecimals;

    void Awake()
    {
        sliderComp = GetComponent<Slider>();
        if (sliderComp == null)
        {
            Debug.LogError("Slider component not found on " + gameObject.name);
        }
    }

    public void SliderValueChange(Text textComponent)
    {
        if (sliderComp == null || textComponent == null)
        {
            Debug.LogWarning($"Missing references in SliderManager on {gameObject.name}");
            return;
        }

        try
        {
            if (noDecimals)
            {
                textComponent.text = sliderComp.value.ToString("N0");
            }
            else
            {
                textComponent.text = sliderComp.value.ToString("F1");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in SliderValueChange: {e.Message}");
        }
    }
}
