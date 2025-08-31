using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Fungsi untuk tombol Play
    public void PlayGame()
    {
        // Reset anomaly spawner if it exists (for retry functionality)
        if (AnomalySpawner.Instance != null)
        {
            AnomalySpawner.Instance.ResetSpawner();
        }
        
        // Reset reeling system if it exists (for retry functionality)
        var reelingSystem = FindFirstObjectByType<Reeling>();
        if (reelingSystem != null)
        {
            reelingSystem.ResetReelingSystem();
        }
        
        // Reset jumpscare system if it exists (for retry functionality)
        if (JumpscareSystem.Instance != null)
        {
            JumpscareSystem.Instance.ResetJumpscareSystem();
        }
        
        // Ganti "GameScene" dengan nama scene utama Anda
        SceneManager.LoadScene("_MainScene");
        // Pause the game
        Time.timeScale = 1f;
    }

    // Fungsi untuk tombol Exit
    public void ExitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
