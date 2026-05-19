using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PauseMenu : MonoBehaviour
{
    public Volume globalVolume;
    public GameObject pauseMenuUI;
    public CanvasGroup hpCanvasGroup;

    private DepthOfField depthOfField;
    private bool isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;

        // Blur
        if (globalVolume.profile.TryGet(out depthOfField))
            depthOfField.active = true;

        // HP semi-transparente
        if (hpCanvasGroup != null)
            hpCanvasGroup.alpha = 0.3f;
    }

    public void Resume()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;

        // Quitar blur
        if (globalVolume.profile.TryGet(out depthOfField))
            depthOfField.active = false;

        // Restaurar HP
        if (hpCanvasGroup != null)
            hpCanvasGroup.alpha = 1f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}