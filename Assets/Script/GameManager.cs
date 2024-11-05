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
        backgroundMusic = Object.FindFirstObjectByType<BackgroundMusic>(); // BackgroundMusic �ν��Ͻ��� ã���ϴ�.
        UpdateKillCountText();
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true); // ���� ���� �г��� Ȱ��ȭ�մϴ�.



        if (backgroundMusic != null)
        {
            backgroundMusic.StopMusic(); // ��� ������ ����ϴ�.
            Debug.Log("��� ������ ���߾����ϴ�."); // Ȯ���� ���� �α�
        }
        else
        {
            Debug.LogWarning("BackgroundMusic �ν��Ͻ��� ã�� �� �����ϴ�."); // �ν��Ͻ��� ���� ��� ��� �α�
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
