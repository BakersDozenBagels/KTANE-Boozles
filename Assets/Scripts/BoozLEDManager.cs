using System.Collections;
using UnityEngine;

public class BoozLEDManager : MonoBehaviour {
    public LED[] leds;

    [SerializeField]
    private float flashTime = 2f;

	// Use this for initialization
	void Start () {
        StartCoroutine(InitLights());
    }
	
	public void ShowState(int[] correct)
    {
        StartCoroutine(FlashState(correct));
    }

    private IEnumerator FlashState(int[] correct)
    { 
        foreach (LED led in leds)
        {
            led.ChangeBrightness(1f);
            led.ChangeMaterial(correct[System.Array.IndexOf(leds, led)]);
        }
        yield return new WaitForSeconds(flashTime);
        foreach (LED led in leds)
        {
            led.ChangeBrightness(0f);
            led.ChangeMaterial(LED.Colors.Red);
        }
    }

    private IEnumerator InitLights()
    {
        yield return null;
        StartCoroutine(FlickerLight1());
        StartCoroutine(FlickerLight2());
        StartCoroutine(FlickerLight3());
        StartCoroutine(FlickerLight4());
        float scalar = transform.lossyScale.x;
        foreach (LED led in leds)
        {
            led.ChangeBrightness(0f);
            led.ChangeMaterial(LED.Colors.Red);
            led.light.range *= scalar;
        }
    }

    private IEnumerator FlickerLight1()
    {
        while (true)
        {
            leds[0].ChangeBrightness(leds[0].light.intensity + 0.05f);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            leds[0].ChangeBrightness(leds[0].light.intensity - 0.06f);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }
    }
    private IEnumerator FlickerLight2()
    {
        while (true)
        {
            leds[1].ChangeBrightness(leds[1].light.intensity + 0.05f);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            leds[1].ChangeBrightness(leds[1].light.intensity - 0.06f);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }
    }
    private IEnumerator FlickerLight3()
    {
        while (true)
        {
            leds[2].ChangeBrightness(leds[2].light.intensity + 0.05f);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            leds[2].ChangeBrightness(leds[2].light.intensity - 0.06f);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }
    }
    private IEnumerator FlickerLight4()
    {
        while (true)
        {
            leds[3].ChangeBrightness(leds[3].light.intensity + 0.05f);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            leds[3].ChangeBrightness(leds[3].light.intensity - 0.06f);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }
    }
}
