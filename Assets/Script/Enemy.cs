
using UnityEngine;
using System.Collections;
using TMPro;

public class Enemy : MonoBehaviour
{
    public int maxHP;
    private int currentHP;
    public bool isDead; // ���� ���� ���¸� üũ�ϴ� ����
    private HpBar hpBar;
    private AudioSource audioSource; // AudioSource �߰�
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
            Debug.LogError("HpBar�� �ʱ�ȭ���� �ʾҽ��ϴ�. ���� �ڽ� ������Ʈ�� Ȯ���ϼ���.");
        }
        
        

    }
    public void AttackAnimation()
    {
        animator.SetTrigger("atk");
        Debug.Log("���� ���� �ִϸ��̼� ����");
        PlayAttackSound();
        
        // �ִϸ��̼� ���� ���� �ڵ� �ۼ�
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
        isDead = false; // �ʱ� ���¿��� ���� ���� ����


        UpdateHpBar(); // �ʱ� HP �� ������Ʈ
        Debug.Log("���� �����Ǿ����ϴ�. HP: " + currentHP);
    }

  

    public void TakeDamage(int damage)
    {
        if (isDead) return; // ���� �̹� �׾����� �� �̻� �������� ���� ����

        currentHP -= damage;
        Debug.Log("������ ������: " + damage + ", ���� HP: " + currentHP);


        UpdateHpBar();

        if (currentHP <= 0)
        {
            Die();
        }
    }
    private void UpdateHpBar()
    {
        float normalizedHP = (float)currentHP / maxHP; // ����ȭ�� HP ���
        Debug.Log("����ȭ�� HP: " + normalizedHP); // �߰��� �α�
        if (hpBar != null)
        {
            hpBar.SetHP(normalizedHP); // HP �� ������Ʈ
        }
    }


    // ���� �׾��� �� ȣ��Ǵ� �޼���
    public void Die()
    {

        isDead = true; // ���� ������ ���¸� ����
        Debug.Log("���� �׾����ϴ�.");
        PlayDeathSound();
        FindFirstObjectByType<Stage>().OnEnemyDeath(); // Stage Ŭ������ ��� �˸�
        FindFirstObjectByType<GameManager>().IncrementKillCount();

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("die"); // �ִϸ��̼� Ʈ���� ����
        }

        // �ִϸ��̼��� ����ǰ� �� �Ŀ� �� ������Ʈ ����
        Destroy(gameObject, 0.6f); // 0.6���� ����
    }
    private void PlayDeathSound()
    {
        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound); // ���� ���
        }
    }
   

}