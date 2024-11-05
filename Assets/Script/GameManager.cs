using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    private BackgroundMusic backgroundMusic;
    public TextMeshProUGUI killCountText;
    private int killCount = 0;

    private void Start()
    {
        backgroundMusic = Object.FindFirstObjectByType<BackgroundMusic>(); // BackgroundMusic 인스턴스를 찾습니다.
        UpdateKillCountText();
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true); // 게임 오버 패널을 활성화합니다.



        if (backgroundMusic != null)
        {
            backgroundMusic.StopMusic(); // 배경 음악을 멈춥니다.
            Debug.Log("배경 음악이 멈추었습니다."); // 확인을 위한 로그
        }
        else
        {
            Debug.LogWarning("BackgroundMusic 인스턴스를 찾을 수 없습니다."); // 인스턴스가 없는 경우 경고 로그
        }
    }
    public void IncrementKillCount()
    {
        killCount++;
        UpdateKillCountText();
    }

    void UpdateKillCountText()
    {
        killCountText.text = "Kills: " + killCount.ToString();
    }
}
