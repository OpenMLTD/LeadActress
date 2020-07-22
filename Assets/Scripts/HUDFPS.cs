using System.Collections;
using UnityEngine;

// https://wiki.unity3d.com/index.php/FramesPerSecond
[AddComponentMenu("Utilities/HUDFPS")]
public class HUDFPS : MonoBehaviour {

    // Attach this to any object to make a frames/second indicator.
    //
    // It calculates frames/second over each updateInterval,
    // so the display does not keep changing wildly.
    //
    // It is also fairly accurate at very low FPS counts (<10).
    // We do this not by simply counting frames per interval, but
    // by accumulating FPS for each frame. This way we end up with
    // correct overall FPS even if the interval renders something like
    // 5.5 frames.

    public Rect startRect = new Rect(10, 10, 75, 50); // The rect the window is initially displayed at.

    public bool updateColor = true; // Do you want the color to change if the FPS gets low

    public bool allowDrag = true; // Do you want to allow the dragging of the FPS window

    public float frequency = 0.5f; // The update frequency of the fps

    public int nbDecimal = 1; // How many decimal do you want to display

    private float accum = 0f; // FPS accumulated over the interval

    private int frames = 0; // Frames drawn over the interval

    private Color color = Color.white; // The color of the GUI, depending of the FPS ( R < 10, Y < 30, G >= 30 )

    private string sFPS = string.Empty; // The fps formatted into a string.

    private GUIStyle style; // The style the text will be displayed at, based en defaultSkin.label.

    private string nbFormat;

    private void Awake() {
        nbFormat = $"f{Mathf.Clamp(nbDecimal, 0, 10)}";
    }

    private void Start() {
        StartCoroutine(CalculateFps());
    }

    private void Update() {
        accum += Time.timeScale / Time.deltaTime;
        ++frames;
    }

    private IEnumerator CalculateFps() {
        // Infinite loop executed every "frequency" seconds.
        while (true) {
            // Update the FPS
            var fps = accum / frames;
            sFPS = fps.ToString(nbFormat);

            //Update the color
            color = (fps >= 30) ? Color.green : ((fps > 10) ? Color.red : Color.yellow);

            accum = 0.0F;
            frames = 0;

            yield return new WaitForSeconds(frequency);
        }

        // ReSharper disable once IteratorNeverReturns
    }

    private void OnGUI() {
        // Copy the default label skin, change the color and the alignment
        if (style == null) {
            style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
        }

        GUI.color = updateColor ? color : Color.white;
        startRect = GUI.Window(0, startRect, DoMyWindow, string.Empty);
    }

    private void DoMyWindow(int windowID) {
        GUI.Label(new Rect(0, 0, startRect.width, startRect.height), $"FPS: {sFPS}", style);

        if (allowDrag) {
            GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
        }
    }

}
