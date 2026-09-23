using System.Collections.Generic;
using UnityEngine;

// 가짜 이동을 기록해 둘 데이터 보관소
public class ChessMove
{
    public int startX, startY;
    public int endX, endY;
    public GameObject capturedPiece; // 잡힌 기물을 기억해뒀다가 되돌릴 때 사용
}

public class ChessAI : MonoBehaviour
{
    public PieceTeam aiTeam = PieceTeam.Black; // AI는 흑(Black)으로 설정

    // 1. 메인 함수: 매니저가 이 함수를 호출하여 AI의 결정을 물어봅니다.
    public ChessMove GetBestMove(GameObject[,] board, int depth)
    {
        List<ChessMove> allMoves = GetAllMoves(board, aiTeam);
        ChessMove bestMove = null;
        int bestScore = int.MinValue;

        foreach (ChessMove move in allMoves)
        {
            DoFakeMove(board, move); // 가짜로 이동해보기
            int score = Minimax(board, depth - 1, false); // 미니맥스 탐색 시작 (다음은 플레이어 턴)
            UndoFakeMove(board, move); // 원상복구

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }
        return bestMove;
    }

    // 2. 미니맥스 알고리즘 본체
    private int Minimax(GameObject[,] board, int depth, bool isMaximizing)
    {
        // 정해진 깊이(수 앞)까지 다 봤거나, 누군가 이겼다면 현재 점수 반환
        if (depth == 0) return EvaluateBoard(board);

        PieceTeam currentTeam = isMaximizing ? aiTeam : PieceTeam.White;
        List<ChessMove> allMoves = GetAllMoves(board, currentTeam);

        if (allMoves.Count == 0) return EvaluateBoard(board);

        if (isMaximizing) // AI의 턴 (최고 점수 찾기)
        {
            int maxEval = int.MinValue;
            foreach (ChessMove move in allMoves)
            {
                DoFakeMove(board, move);
                int eval = Minimax(board, depth - 1, false);
                UndoFakeMove(board, move);
                maxEval = Mathf.Max(maxEval, eval);
            }
            return maxEval;
        }
        else // 플레이어의 턴 (AI 입장에선 최악의 점수 찾기)
        {
            int minEval = int.MaxValue;
            foreach (ChessMove move in allMoves)
            {
                DoFakeMove(board, move);
                int eval = Minimax(board, depth - 1, true);
                UndoFakeMove(board, move);
                minEval = Mathf.Min(minEval, eval);
            }
            return minEval;
        }
    }

    // 3. 체스판 가치 평가 (AI 점수 계산기)
    private int EvaluateBoard(GameObject[,] board)
    {
        int totalScore = 0;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board[x, y] != null)
                {
                    ChessPiece piece = board[x, y].GetComponent<ChessPiece>();
                    int pieceValue = GetPieceValue(piece);
                    
                    // AI 기물이면 플러스, 플레이어 기물이면 마이너스
                    if (piece.team == aiTeam) totalScore += pieceValue;
                    else totalScore -= pieceValue;
                }
            }
        }
        return totalScore;
    }

    private int GetPieceValue(ChessPiece piece)
    {
        if (piece is Pawn) return 10;
        if (piece is Knight) return 30;
        if (piece is Bishop) return 30;
        if (piece is Rook) return 50;
        if (piece is Queen) return 90;
        if (piece is King) return 900;
        return 0;
    }

    // --- 아래는 배열 데이터를 조작하는 유틸리티 함수들 ---

    private List<ChessMove> GetAllMoves(GameObject[,] board, PieceTeam team)
    {
        List<ChessMove> moves = new List<ChessMove>();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board[x, y] != null)
                {
                    ChessPiece piece = board[x, y].GetComponent<ChessPiece>();
                    if (piece.team == team)
                    {
                        List<Vector2Int> available = piece.GetAvailableMoves(board);
                        foreach (Vector2Int target in available)
                        {
                            ChessMove move = new ChessMove { startX = x, startY = y, endX = target.x, endY = target.y, capturedPiece = board[target.x, target.y] };
                            moves.Add(move);
                        }
                    }
                }
            }
        }
        return moves;
    }

    private void DoFakeMove(GameObject[,] board, ChessMove move)
    {
        board[move.endX, move.endY] = board[move.startX, move.startY];
        board[move.startX, move.startY] = null;
        
        ChessPiece piece = board[move.endX, move.endY].GetComponent<ChessPiece>();
        piece.currentX = move.endX; piece.currentY = move.endY;
    }

    private void UndoFakeMove(GameObject[,] board, ChessMove move)
    {
        board[move.startX, move.startY] = board[move.endX, move.endY];
        board[move.endX, move.endY] = move.capturedPiece; // 잡았던 기물 부활
        
        ChessPiece piece = board[move.startX, move.startY].GetComponent<ChessPiece>();
        piece.currentX = move.startX; piece.currentY = move.startY;
    }
}