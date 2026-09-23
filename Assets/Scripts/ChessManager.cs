using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // 최신 입력 시스템 사용

public class ChessManager : MonoBehaviour
{
    [Header("기물 프리팹 (순서: 0폰, 1룩, 2나이트, 3비숍, 4퀸, 5킹)")]
    public GameObject[] whitePrefabs;
    public GameObject[] blackPrefabs;

    [Header("보드 좌표 설정")]
    public float tileSize = 0.625f;
    public Vector2 boardOrigin = new Vector2(-2.1875f, -2.1875f);

    // 상태 관리 변수들
    private GameObject[,] pieces = new GameObject[8, 8];
    private Vector2Int selectedPiecePos = new Vector2Int(-1, -1);
    
    // 현재 턴 (백부터 시작)
    private PieceTeam currentTurn = PieceTeam.White;
    
    // 게임 종료 여부
    private bool isGameOver = false;

    void Start()
    {
        SpawnAllPieces();
    }

    void Update()
    {
        // 게임이 종료되었다면 더 이상 클릭을 받지 않습니다.
        if (isGameOver) return; 

        // 최신 입력 시스템: 마우스 좌클릭 감지
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) 
        {
            // 화면 좌표를 2D 게임 월드 좌표로 변환 (Z축 깊이 보정)
            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            
            // 월드 좌표를 배열 인덱스로 역산
            Vector2Int gridPos = GetGridPosition(mouseWorldPos);

            // 체스판 밖을 클릭했으면 무시
            if (gridPos.x < 0 || gridPos.x > 7 || gridPos.y < 0 || gridPos.y > 7) return;

            // [1] 기물이 선택되지 않은 상태일 때
            if (selectedPiecePos.x == -1) 
            {
                if (pieces[gridPos.x, gridPos.y] != null) 
                {
                    // 클릭한 기물이 '현재 턴'의 팀이 아니라면 무시
                    if (pieces[gridPos.x, gridPos.y].GetComponent<ChessPiece>().team != currentTurn) return;

                    selectedPiecePos = gridPos; 
                    pieces[gridPos.x, gridPos.y].transform.localScale = new Vector3(1.2f, 1.2f, 1f); 
                }
            }
            // [2] 기물이 이미 선택된 상태에서 클릭했을 때 (이동 명령)
            else 
            {
                // 같은 기물을 다시 누르면 선택 취소
                if (selectedPiecePos == gridPos)
                {
                    pieces[selectedPiecePos.x, selectedPiecePos.y].transform.localScale = Vector3.one;
                    selectedPiecePos = new Vector2Int(-1, -1);
                    return;
                }

                ChessPiece pieceScript = pieces[selectedPiecePos.x, selectedPiecePos.y].GetComponent<ChessPiece>();
                List<Vector2Int> validMoves = pieceScript.GetAvailableMoves(pieces);

                // 유효한 이동 칸일 때만 이동
                if (validMoves.Contains(gridPos))
                {
                    MovePiece(selectedPiecePos.x, selectedPiecePos.y, gridPos.x, gridPos.y);
                    pieceScript.currentX = gridPos.x;
                    pieceScript.currentY = gridPos.y;

                    // 이동이 끝났고 게임이 끝나지 않았다면 턴을 넘김
                    if (!isGameOver)
                    {
                        currentTurn = (currentTurn == PieceTeam.White) ? PieceTeam.Black : PieceTeam.White;
                    }
                }
                else
                {
                    Debug.Log("그곳으로는 이동할 수 없습니다!");
                }
                
                pieceScript.transform.localScale = Vector3.one; 
                selectedPiecePos = new Vector2Int(-1, -1); 
            }
        }
    }

    private void MovePiece(int startX, int startY, int endX, int endY)
    {
        GameObject movingPiece = pieces[startX, startY];

        // 목표 칸에 기물이 있다면 잡기 처리
        if (pieces[endX, endY] != null)
        {
            GameObject capturedPiece = pieces[endX, endY];

            // [핵심] 잡힌 기물이 '킹'인지 컴포넌트로 검사
            if (capturedPiece.GetComponent<King>() != null)
            {
                Debug.Log($"<b>{currentTurn} 팀 승리!</b> 게임이 종료되었습니다.");
                isGameOver = true; // Update 문의 클릭 감지를 차단
            }

            Destroy(capturedPiece);
        }

        // 배열 데이터 업데이트
        pieces[endX, endY] = movingPiece;
        pieces[startX, startY] = null;

        // 실제 화면 위치를 목표 칸으로 이동
        movingPiece.transform.position = GetWorldPosition(endX, endY);
    }

    private void SpawnAllPieces()
    {
        SpawnTeam(0, whitePrefabs, PieceTeam.White);
        SpawnTeam(7, blackPrefabs, PieceTeam.Black);
    }

    private void SpawnTeam(int backRowY, GameObject[] prefabs, PieceTeam team)
    {
        int pawnRowY = (backRowY == 0) ? 1 : 6;

        for (int x = 0; x < 8; x++) SpawnSingle(x, pawnRowY, prefabs[0], team); // 폰

        SpawnSingle(0, backRowY, prefabs[1], team); SpawnSingle(7, backRowY, prefabs[1], team); // 룩
        SpawnSingle(1, backRowY, prefabs[2], team); SpawnSingle(6, backRowY, prefabs[2], team); // 나이트
        SpawnSingle(2, backRowY, prefabs[3], team); SpawnSingle(5, backRowY, prefabs[3], team); // 비숍
        SpawnSingle(3, backRowY, prefabs[4], team); // 퀸
        SpawnSingle(4, backRowY, prefabs[5], team); // 킹
    }

    private void SpawnSingle(int x, int y, GameObject prefab, PieceTeam team)
    {
        GameObject newPiece = Instantiate(prefab, GetWorldPosition(x, y), Quaternion.identity);
        pieces[x, y] = newPiece;

        ChessPiece pieceScript = newPiece.GetComponent<ChessPiece>();
        if (pieceScript != null)
        {
            pieceScript.currentX = x;
            pieceScript.currentY = y;
            pieceScript.team = team; 
        }
    }

    // 기물 생성 및 이동 시 '칸의 정중앙'을 반환
    private Vector2 GetWorldPosition(int x, int y)
    {
        float worldX = boardOrigin.x + (x * tileSize);
        float worldY = boardOrigin.y + (y * tileSize);
        return new Vector2(worldX, worldY);
    }

    // 클릭 판정 시 '칸의 왼쪽 아래 모서리'를 기준으로 100% 정확하게 배열 인덱스 반환
    private Vector2Int GetGridPosition(Vector2 worldPos)
    {
        float startX = boardOrigin.x - (tileSize / 2f);
        float startY = boardOrigin.y - (tileSize / 2f);

        int x = Mathf.FloorToInt((worldPos.x - startX) / tileSize);
        int y = Mathf.FloorToInt((worldPos.y - startY) / tileSize);
        return new Vector2Int(x, y);
    }
}