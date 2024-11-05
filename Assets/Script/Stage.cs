using UnityEngine;
using TMPro;
using System.Collections;

public class Stage : MonoBehaviour
{
    [Header("Editor Objects")]
    public GameObject tilePrefab;
    public GameObject enemyPrefab; // 적 프리팹 추가
    public Transform backgroundNode;
    public Transform boardNode;
    public Transform tetrominoNode;
    public GameObject gameoverPanel;
    public TextMeshProUGUI result;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    private float enemySpawnTimer;
    private float attackTimer = 10f; // 기본 공격 타이머를 10초로 설정
    public float attackDelay = 10f; // 공격 주기 10초
    private int currentEnemyDamage;


    private Enemy currentEnemy; // 현재 생성된 적
    private int baseEnemyMaxHP = 100; // 최초 적의 HP 값
    private int baseEnemydamage =1;

    private int score = 0;
    private Player player;

    public Vector2 enemyPosition = new Vector2(14, 0);

    void IncreaseScore(int amount)
    {
        score += amount;
        UpdateScoreText();
        Debug.Log("현재 점수: " + score);

        if (score % 300 == 0)
        {
            HealPlayer(1); // 플레이어 체력 1 회복
            UpdatePlayerHPBar();
        }


        if (currentEnemy != null)
        {
            currentEnemy.TakeDamage(100); // 100데미지
        }
        attackTimer = attackDelay;
        TriggerAttackAnimation();
        Player player = Object.FindFirstObjectByType<Player>();
        if (player != null && player.currentHealth <= 0) // 플레이어의 체력이 0이면
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
    public float timeLimit = 120f; // 제한 시간 (초)
    private float timeRemaining;

    private GameObject enemy; // 적 변수 추가

    private void Start()
    {
        gameoverPanel.SetActive(false);
        score = 0;
        UpdateScoreText();
        timeRemaining = timeLimit; // 타이머 초기화
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
        attackTimer = attackDelay; // 공격 타이머 초기화



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
            // 5초마다 자동 공격
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                AttackEnemy(); // 적이 공격
                attackTimer = attackDelay; // 타이머 초기화
            }
        }
        // 시간 경과에 따라 공격 주기 감소
        


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

            if (currentEnemy == null) // 적이 죽으면 새로운 적 생성
            {
                SpawnEnemy(currentEnemy.maxHP * 2, currentEnemyDamage+1); // 직전 적 HP의 2배로 생성
            }
        }



    }


    void UpdateTimeText()
    {
        timeText.text = "Time: " + Mathf.Ceil(timeRemaining).ToString(); // 남은 시간 표시
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
                timeRemaining += 10f; // 남은 시간 10초 증가
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
            // I : 하늘색
            case 0:
                color = new Color32(115, 251, 253, 255);
                CreateTile(tetrominoNode, new Vector2(-2f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                break;
            // J : 파란색
            case 1:
                color = new Color32(25, 25, 255, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(-1f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 1.0f), color);
                break;
            // L : 주황색
            case 2:
                color = new Color32(255, 127, 0, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                break;
            // O : 노란색
            case 3:
                color = new Color32(255, 255, 0, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(-1f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 1.0f), color);
                break;
            // S : 초록색
            case 4:
                color = new Color32(0, 255, 0, 255);
                CreateTile(tetrominoNode, new Vector2(0.0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(-1f, 1.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 1.0f), color);
                break;
            // T : 보라색
            case 5:
                color = new Color32(127, 0, 255, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0.0f, 1.0f), color);
                break;
            // Z : 빨간색
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
        if (currentEnemy != null && !currentEnemy.isDead) // 현재 적이 없거나 이미 죽었으면 재생성하지 않음
        {
            StartCoroutine(SpawnEnemyAfterDelay(0.6f)); // 0.6초 후에 새로운 적 생성
        }
    }

    private IEnumerator SpawnEnemyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // 지정된 시간 동안 대기
        int newEnemyMaxHP = (currentEnemy.maxHP * 2); // HP를 두 배로 증가
        int newEnemyDamage = currentEnemyDamage;

        SpawnEnemy(newEnemyMaxHP, newEnemyDamage); // 새로운 적 생성
    }

    private void SpawnEnemy(int maxHP, int damage)
    {
        GameObject newEnemy = Instantiate(enemyPrefab); // 적 생성
        currentEnemy = newEnemy.GetComponent<Enemy>();
        currentEnemy.maxHP = maxHP; // 적의 HP 설정
        currentEnemy.damage = damage; // 적의 데미지 설정
        currentEnemy.Initialize(); // 적 초기화
    }

    void TriggerAttackAnimation()
    {
        Player player = Object.FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.AttackAnimation();
            Debug.Log("공격 애니메이션 실행");
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
                Debug.Log("적이 플레이어를 공격했습니다!");

            }
        }
    }
    private void HealPlayer(int amount)
    {
        player.currentHealth += amount; // 플레이어 체력 증가
        Debug.Log("플레이어가 회복되었습니다. 현재 체력: " + player.currentHealth);

        if (player.currentHealth > player.maxHealth) // maxHP를 maxHealth로 변경
        {
            player.currentHealth = player.maxHealth; // 최대값 초과 시 최대 HP로 설정
        }
    }
    void UpdatePlayerHPBar()
    {
        // Player 클래스의 인스턴스에 접근하여 HP 바를 갱신하는 코드
        // 예를 들어, Player 인스턴스를 Stage 클래스의 변수로 가지고 있을 경우:
        player.UpdateHpBar(); // 또는 player.hpBar.Update(); 등
    }
}



