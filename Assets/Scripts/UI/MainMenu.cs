using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Fungsi untuk tombol Play
    public void PlayGame()
    {
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
