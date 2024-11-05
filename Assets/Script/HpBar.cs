using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    public Image hpBarImage; // HP �� �̹���

    public void SetHP(float normalizedHP)
    {
        hpBarImage.fillAmount = normalizedHP; // HP ���� ũ�� ������Ʈ
        Debug.Log("HP �� ������Ʈ: " + normalizedHP); // �߰��� �α�
    }
}