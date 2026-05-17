using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurController : MonoBehaviour
{
    public Volume globalVolume;
    private DepthOfField depthOfField;

    private bool toggleDof;
    public float focusDistance;

    public GameObject pause;

    private void ToggleBackGroundUI()
    {
        toggleDof = !toggleDof;
        if (globalVolume.profile.TryGet(out depthOfField))
        {
            depthOfField.active = toggleDof;
            depthOfField.focusDistance.value = focusDistance;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleBackGroundUI();
            pause.SetActive(toggleDof);

            if (toggleDof)
                Time.timeScale = 0f;
            else
                Time.timeScale = 1f;
        }
    }
}
