using UnityEngine;
using System.Collections;
using UnityEngine.UI;
        

public class OrbValueChanger : MonoBehaviour {

    public Slider thisSlider;
    Material thisOrbMaterial;

	// Use this for initialization
	void Start () {

        thisOrbMaterial = this.GetComponent<Image>().material;
        ChangeOrbValue();

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ChangeOrbValue()
    {
        thisOrbMaterial.SetFloat("_Value", thisSlider.value);
    }
}
