using UnityEngine;
using System.Collections; // IEnumerator�� ����ϱ� ���� �߰�

public class Player : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource; // AudioSource �߰�
    public AudioClip attackSound; // ���� ���� Ŭ���� ���� ����
    public int maxHealth = 20; // �÷��̾��� �ʱ� ü��
    private Stage stage;
    public int currentHealth;
    public HpBar hpBar;



    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        stage = FindFirstObjectByType<Stage>();

        currentHealth = maxHealth; // �ʱ� ü�� ����
        hpBar = GetComponentInChildren<HpBar>();
        UpdateHpBar();


    }

    public void UpdateHpBar()
    {
        float normalizedHP = (float)currentHealth / maxHealth; // ����ȭ�� HP ���
        if (hpBar != null)
        {
            hpBar.SetHP(normalizedHP); // HP �� ������Ʈ
        }
    }
        public void AttackAnimation()
    {
        animator.SetTrigger("atk");
        PlayAttackSound();
        StartCoroutine(ReturnToIdle()); // �ִϸ��̼� �� Idle�� ����
    }
    private void PlayAttackSound()
    {
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound); // ���� ���
        }
    }


    private IEnumerator ReturnToIdle()
    {
        // ���� �ִϸ��̼��� ���̸�ŭ ���
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        IdleAnimation(); // Idle �ִϸ��̼� ����
    }

    public void IdleAnimation()
    {
        animator.SetTrigger("idle");
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("�÷��̾ �������� �޾ҽ��ϴ�. ���� ü��: " + currentHealth);
        UpdateHpBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // �÷��̾ �׾��� ���� ó��
        Debug.Log("�÷��̾ ����߽��ϴ�.");
        stage.GameOver();
    }
 
}
  


