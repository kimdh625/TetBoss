using UnityEngine;
using System.Collections; // IEnumerator를 사용하기 위해 추가

public class Player : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource; // AudioSource 추가
    public AudioClip attackSound; // 공격 사운드 클립을 위한 변수
    public int maxHealth = 20; // 플레이어의 초기 체력
    private Stage stage;
    public int currentHealth;
    public HpBar hpBar;



    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        stage = FindFirstObjectByType<Stage>();

        currentHealth = maxHealth; // 초기 체력 설정
        hpBar = GetComponentInChildren<HpBar>();
        UpdateHpBar();


    }

    public void UpdateHpBar()
    {
        float normalizedHP = (float)currentHealth / maxHealth; // 정규화된 HP 계산
        if (hpBar != null)
        {
            hpBar.SetHP(normalizedHP); // HP 바 업데이트
        }
    }
        public void AttackAnimation()
    {
        animator.SetTrigger("atk");
        PlayAttackSound();
        StartCoroutine(ReturnToIdle()); // 애니메이션 후 Idle로 복귀
    }
    private void PlayAttackSound()
    {
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound); // 사운드 재생
        }
    }


    private IEnumerator ReturnToIdle()
    {
        // 공격 애니메이션의 길이만큼 대기
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        IdleAnimation(); // Idle 애니메이션 실행
    }

    public void IdleAnimation()
    {
        animator.SetTrigger("idle");
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("플레이어가 데미지를 받았습니다. 현재 체력: " + currentHealth);
        UpdateHpBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 플레이어가 죽었을 때의 처리
        Debug.Log("플레이어가 사망했습니다.");
        stage.GameOver();
    }
 
}
  


