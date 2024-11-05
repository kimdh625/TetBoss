using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    public Image hpBarImage; // HP 바 이미지

    public void SetHP(float normalizedHP)
    {
        hpBarImage.fillAmount = normalizedHP; // HP 바의 크기 업데이트
        Debug.Log("HP 바 업데이트: " + normalizedHP); // 추가된 로그
    }
}