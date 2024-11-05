
using UnityEngine;
using System.Collections;
using TMPro;

public class Enemy : MonoBehaviour
{
    public int maxHP;
    private int currentHP;
    public bool isDead; // 적의 생사 상태를 체크하는 변수
    private HpBar hpBar;
    private AudioSource audioSource; // AudioSource 추가
    public AudioClip deathSound;
    private Animator animator;
    public AudioClip attackSound;
    public int damage = 1;




    void Start()
    {
        Initialize();
        hpBar = GetComponentInChildren<HpBar>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        if (hpBar == null)
        {
            Debug.LogError("HpBar가 초기화되지 않았습니다. 적의 자식 오브젝트를 확인하세요.");
        }
        
        

    }
    public void AttackAnimation()
    {
        animator.SetTrigger("atk");
        Debug.Log("적의 공격 애니메이션 실행");
        PlayAttackSound();
        
        // 애니메이션 실행 관련 코드 작성
    }
    private void PlayAttackSound()
    {
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }
    public void ReturnToIdle()
    {
        animator.SetTrigger("idle");
    }



public void Initialize()
    {
        currentHP = maxHP;
        isDead = false; // 초기 상태에서 적은 죽지 않음


        UpdateHpBar(); // 초기 HP 바 업데이트
        Debug.Log("적이 생성되었습니다. HP: " + currentHP);
    }

  

    public void TakeDamage(int damage)
    {
        if (isDead) return; // 적이 이미 죽었으면 더 이상 데미지를 받지 않음

        currentHP -= damage;
        Debug.Log("적에게 데미지: " + damage + ", 현재 HP: " + currentHP);


        UpdateHpBar();

        if (currentHP <= 0)
        {
            Die();
        }
    }
    private void UpdateHpBar()
    {
        float normalizedHP = (float)currentHP / maxHP; // 정규화된 HP 계산
        Debug.Log("정규화된 HP: " + normalizedHP); // 추가된 로그
        if (hpBar != null)
        {
            hpBar.SetHP(normalizedHP); // HP 바 업데이트
        }
    }


    // 적이 죽었을 때 호출되는 메서드
    public void Die()
    {

        isDead = true; // 적이 죽으면 상태를 변경
        Debug.Log("적이 죽었습니다.");
        PlayDeathSound();
        FindFirstObjectByType<Stage>().OnEnemyDeath(); // Stage 클래스에 사망 알림
        FindFirstObjectByType<GameManager>().IncrementKillCount();

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("die"); // 애니메이션 트리거 실행
        }

        // 애니메이션이 재생되고 난 후에 적 오브젝트 삭제
        Destroy(gameObject, 0.6f); // 0.6초후 삭제
    }
    private void PlayDeathSound()
    {
        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound); // 사운드 재생
        }
    }
   

}