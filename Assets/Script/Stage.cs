using UnityEngine;
using TMPro;
using System.Collections;

public class Stage : MonoBehaviour
{
    [Header("Editor Objects")]
    public GameObject tilePrefab;
    public GameObject enemyPrefab; // �� ������ �߰�
    public Transform backgroundNode;
    public Transform boardNode;
    public Transform tetrominoNode;
    public GameObject gameoverPanel;
    public TextMeshProUGUI result;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    private float enemySpawnTimer;
    private float attackTimer = 10f; // �⺻ ���� Ÿ�̸Ӹ� 10�ʷ� ����
    public float attackDelay = 10f; // ���� �ֱ� 10��
    private int currentEnemyDamage;


    private Enemy currentEnemy; // ���� ������ ��
    private int baseEnemyMaxHP = 100; // ���� ���� HP ��
    private int baseEnemydamage =1;

    private int score = 0;
    private Player player;

    public Vector2 enemyPosition = new Vector2(14, 0);

    void IncreaseScore(int amount)
    {
        score += amount;
        UpdateScoreText();
        Debug.Log("���� ����: " + score);

        if (score % 300 == 0)
        {
            HealPlayer(1); // �÷��̾� ü�� 1 ȸ��
            UpdatePlayerHPBar();
        }


        if (currentEnemy != null)
        {
            currentEnemy.TakeDamage(100); // 100������
        }
        attackTimer = attackDelay;
        TriggerAttackAnimation();
        Player player = Object.FindFirstObjectByType<Player>();
        if (player != null && player.currentHealth <= 0) // �÷��̾��� ü���� 0�̸�
        {
            GameOver();
        }
        currentEnemyDamage += 1;

    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    [Header("Game Settings")]
    [Range(4, 40)]
    public int boardWidth = 10;
    [Range(5, 20)]
    public int boardHeight = 20;
    public float fallCycle = 1.0f;

    private int halfWidth;
    private int halfHeight;
    private float nextFallTime;

    [Header("Time Settings")]
    public float timeLimit = 120f; // ���� �ð� (��)
    private float timeRemaining;

    private GameObject enemy; // �� ���� �߰�

    private void Start()
    {
        gameoverPanel.SetActive(false);
        score = 0;
        UpdateScoreText();
        timeRemaining = timeLimit; // Ÿ�̸� �ʱ�ȭ
        UpdateTimeText();

        halfWidth = Mathf.RoundToInt(boardWidth * 0.5f);
        halfHeight = Mathf.RoundToInt(boardHeight * 0.5f);
        player = Object.FindFirstObjectByType<Player>();

        nextFallTime = Time.time + fallCycle;

        CreateBackground();
        for (int i = 0; i < boardHeight; ++i)
        {
            var col = new GameObject((boardHeight - i - 1).ToString());
            col.transform.position = new Vector3(0, halfHeight - i, 0);
            col.transform.parent = boardNode;
        }

        CreateTetromino();
        SpawnEnemy(baseEnemyMaxHP, baseEnemydamage);
        attackTimer = attackDelay; // ���� Ÿ�̸� �ʱ�ȭ



    }

    private void Update()
    {
        if (gameoverPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Time.timeScale = 1;
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);

            }
            return;


        }
        else
        {
            // 5�ʸ��� �ڵ� ����
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                AttackEnemy(); // ���� ����
                attackTimer = attackDelay; // Ÿ�̸� �ʱ�ȭ
            }
        }
        // �ð� ����� ���� ���� �ֱ� ����
        


        {
            Vector3 moveDir = Vector3.zero;
            bool isRotate = false;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                moveDir.x = -1;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                moveDir.x = 1;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                isRotate = true;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                moveDir.y = -1;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                while (MoveTetromino(Vector3.down, false))
                {
                }
            }

            if (Time.time > nextFallTime)
            {
                nextFallTime = Time.time + fallCycle;
                moveDir = Vector3.down;
                isRotate = false;
            }

            if (moveDir != Vector3.zero || isRotate)
            {
                MoveTetromino(moveDir, isRotate);
            }

            if (!gameoverPanel.activeSelf)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining <= 0)
                {
                    timeRemaining = 0;
                    GameOver();
                }
                UpdateTimeText();
            }

            if (currentEnemy == null) // ���� ������ ���ο� �� ����
            {
                SpawnEnemy(currentEnemy.maxHP * 2, currentEnemyDamage+1); // ���� �� HP�� 2��� ����
            }
        }



    }


    void UpdateTimeText()
    {
        timeText.text = "Time: " + Mathf.Ceil(timeRemaining).ToString(); // ���� �ð� ǥ��
    }

    public void GameOver()
    {
        gameoverPanel.SetActive(true);
        Time.timeScale = 0;
    }

    bool MoveTetromino(Vector3 moveDir, bool isRotate)
    {
        Vector3 oldPos = tetrominoNode.transform.position;
        Quaternion oldRot = tetrominoNode.transform.rotation;

        tetrominoNode.transform.position += moveDir;
        if (isRotate)
        {
            tetrominoNode.transform.rotation *= Quaternion.Euler(0, 0, 90);
        }
        if (!CanMoveTo(tetrominoNode))
        {
            tetrominoNode.transform.position = oldPos;
            tetrominoNode.transform.rotation = oldRot;
            if ((int)moveDir.y == -1 && (int)moveDir.x == 0 && isRotate == false)
            {
                AddToBoard(tetrominoNode);
                CheckBoardColumn();
                CreateTetromino();
            }
            if (!CanMoveTo(tetrominoNode))
            {
                gameoverPanel.SetActive(true);
            }

            return false;
        }

        return true;
    }

    public void AddToBoard(Transform root)
    {
        while (root.childCount > 0)
        {
            var node = root.GetChild(0);

            int x = Mathf.RoundToInt(node.transform.position.x + halfWidth);
            int y = Mathf.RoundToInt(node.transform.position.y + halfHeight - 1);

            node.parent = boardNode.Find(y.ToString());
            node.name = x.ToString();
        }
    }

    void CheckBoardColumn()
    {
        bool isCleared = false;

        foreach (Transform column in boardNode)
        {
            if (column.childCount == boardWidth)
            {
                foreach (Transform tile in column)
                {
                    Destroy(tile.gameObject);
                }

                column.DetachChildren();
                isCleared = true;

                IncreaseScore(100);
                timeRemaining += 10f; // ���� �ð� 10�� ����
            }
        }

        if (isCleared)
        {



            for (int i = 1; i < boardNode.childCount; ++i)
            {
                var column = boardNode.Find(i.ToString());

                if (column.childCount == 0)
                    continue;

                int emptyCol = 0;
                int j = i - 1;
                while (j >= 0)
                {
                    if (boardNode.Find(j.ToString()).childCount == 0)
                    {
                        emptyCol++;
                    }
                    j--;
                }

                if (emptyCol > 0)
                {
                    var targetColumn = boardNode.Find((i - emptyCol).ToString());

                    while (column.childCount > 0)
                    {
                        Transform tile = column.GetChild(0);
                        tile.parent = targetColumn;
                        tile.transform.position += new Vector3(0, -emptyCol, 0);
                    }
                    column.DetachChildren();
                }
            }
        }
    }

    bool CanMoveTo(Transform root)
    {
        for (int i = 0; i < root.childCount; ++i)
        {
            var node = root.GetChild(i);
            int x = Mathf.RoundToInt(node.transform.position.x + halfWidth);
            int y = Mathf.RoundToInt(node.transform.position.y + halfHeight - 1);

            if (x < 0 || x > boardWidth - 1)
                return false;

            if (y < 0)
                return false;

            var column = boardNode.Find(y.ToString());

            if (column != null && column.Find(x.ToString()) != null)
                return false;
        }

        return true;
    }

    Tile CreateTile(Transform parent, Vector2 position, Color color, int order = 1)
    {
        var go = Instantiate(tilePrefab);
        go.transform.parent = parent;
        go.transform.localPosition = position;

        var tile = go.GetComponent<Tile>();
        tile.color = color;
        tile.sortingOrder = order;

        return tile;
    }

    void CreateBackground()
    {
        Color color = Color.gray;

        color.a = 0.5f;
        for (int x = -halfWidth; x < halfWidth; ++x)
        {
            for (int y = halfHeight; y > -halfHeight; --y)
            {
                CreateTile(backgroundNode, new Vector2(x, y), color, 0);
            }
        }

        color.a = 1.0f;
        for (int y = halfHeight; y > -halfHeight; --y)
        {
            CreateTile(backgroundNode, new Vector2(-halfWidth - 1, y), color, 0);
            CreateTile(backgroundNode, new Vector2(halfWidth, y), color, 0);
        }

        for (int x = -halfWidth - 1; x <= halfWidth; ++x)
        {
            CreateTile(backgroundNode, new Vector2(x, -halfHeight), color, 0);
        }
    }

    void CreateTetromino()
    {
        int index = Random.Range(0, 7);
        Color32 color = Color.white;

        tetrominoNode.rotation = Quaternion.identity;
        tetrominoNode.position = new Vector2(0, halfHeight);

        switch (index)
        {
            // I : �ϴû�
            case 0:
                color = new Color32(115, 251, 253, 255);
                CreateTile(tetrominoNode, new Vector2(-2f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                break;
            // J : �Ķ���
            case 1:
                color = new Color32(25, 25, 255, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(-1f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 1.0f), color);
                break;
            // L : ��Ȳ��
            case 2:
                color = new Color32(255, 127, 0, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                break;
            // O : �����
            case 3:
                color = new Color32(255, 255, 0, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(-1f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 1.0f), color);
                break;
            // S : �ʷϻ�
            case 4:
                color = new Color32(0, 255, 0, 255);
                CreateTile(tetrominoNode, new Vector2(0.0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(-1f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 1.0f), color);
                break;
            // T : �����
            case 5:
                color = new Color32(127, 0, 255, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 1.0f), color);
                break;
            // Z : ������
            case 6:
                color = new Color32(255, 0, 0, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 1.0f), color);
                break;
        }
    }



    public void OnEnemyDeath()
    {
        if (currentEnemy != null && !currentEnemy.isDead) // ���� ���� ���ų� �̹� �׾����� ��������� ����
        {
            StartCoroutine(SpawnEnemyAfterDelay(0.6f)); // 0.6�� �Ŀ� ���ο� �� ����
        }
    }

    private IEnumerator SpawnEnemyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // ������ �ð� ���� ���
        int newEnemyMaxHP = (currentEnemy.maxHP * 2); // HP�� �� ��� ����
        int newEnemyDamage = currentEnemyDamage;

        SpawnEnemy(newEnemyMaxHP, newEnemyDamage); // ���ο� �� ����
    }

    private void SpawnEnemy(int maxHP, int damage)
    {
        GameObject newEnemy = Instantiate(enemyPrefab); // �� ����
        currentEnemy = newEnemy.GetComponent<Enemy>();
        currentEnemy.maxHP = maxHP; // ���� HP ����
        currentEnemy.damage = damage; // ���� ������ ����
        currentEnemy.Initialize(); // �� �ʱ�ȭ
    }

    void TriggerAttackAnimation()
    {
        Player player = Object.FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.AttackAnimation();
            Debug.Log("���� �ִϸ��̼� ����");
        }
    }
    void AttackEnemy()
    {

        if (currentEnemy != null)
        {
            currentEnemy.AttackAnimation();
            Player player = Object.FindFirstObjectByType<Player>();
            if (player != null)
            {
                player.TakeDamage(currentEnemy.damage);
                Debug.Log("���� �÷��̾ �����߽��ϴ�!");

            }
        }
    }
    private void HealPlayer(int amount)
    {
        player.currentHealth += amount; // �÷��̾� ü�� ����
        Debug.Log("�÷��̾ ȸ���Ǿ����ϴ�. ���� ü��: " + player.currentHealth);

        if (player.currentHealth > player.maxHealth) // maxHP�� maxHealth�� ����
        {
            player.currentHealth = player.maxHealth; // �ִ밪 �ʰ� �� �ִ� HP�� ����
        }
    }
    void UpdatePlayerHPBar()
    {
        // Player Ŭ������ �ν��Ͻ��� �����Ͽ� HP �ٸ� �����ϴ� �ڵ�
        // ���� ���, Player �ν��Ͻ��� Stage Ŭ������ ������ ������ ���� ���:
        player.UpdateHpBar(); // �Ǵ� player.hpBar.Update(); ��
    }
}



