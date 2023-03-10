using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess3
{
    
    internal static class GameHandler
    {
        internal static Piece?[,] board = new Piece[8,8];
        internal static List<Piece> blackPieces = new List<Piece>();
        internal static List<Piece> whitePieces = new List<Piece>();

        internal static bool pawnProm = false;

        // Cleans the board and sets up the pieces ready for a new game
        internal static void setupGame()
        {
            // Clear old game
            board = new Piece[8, 8];
            blackPieces.Clear();
            whitePieces.Clear();
            MoveCalculation.otherMoves.Clear();
            // Setup new game
            for (int i = 0; i < 8; i++)
            {
                Pawn bPawn = new Pawn(Team.BLACK);
                board[i, 1] = bPawn;
                blackPieces.Add(bPawn);

                Pawn wPawn = new Pawn(Team.WHITE);
                board[i, 6] = wPawn;
                whitePieces.Add(wPawn);
            }
            for(int i = 0; i < 8; i += 7)
            {
                Team tm = i == 0 ? Team.BLACK : Team.WHITE;
                board[0, i] = new Rook(tm);
                board[1, i] = new Knight(tm);
                board[2, i] = new Bishop(tm);
                board[3, i] = new Queen(tm);
                board[4, i] = new King(tm);
                board[5, i] = new Bishop(tm);
                board[6, i] = new Knight(tm);
                board[7, i] = new Rook(tm);
                for (int j = 0; j < 8; j++)
                {
                    if (tm == Team.BLACK)
                        blackPieces.Add(board[j, i]);
                    else
                        whitePieces.Add(board[j, i]);
                }
            }
            foreach(Piece pc in blackPieces)
            {
                MoveCalculation.otherMoves.Add(pc, pc.calculateMoves(false));
            }
        }

        // Takes a piece, and returns a point representing its position on the board
        internal static Point getPiecePos(Piece pc)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board[i, j] == pc)
                        return new Point(i, j);
                }
            }
            return new Point(0,0);
        }
        
        // Moves a piece to a desination point on the board
        internal static StringBuilder movePiece(Piece pt, Point dest)
        {
            Piece? replaced = board[dest.X, dest.Y];
            Point origin = pt.position;
            board[origin.X, origin.Y] = null;
            board[dest.X, dest.Y] = pt;
            Point travel = new Point(dest.X - origin.X, dest.Y - origin.Y);

            StringBuilder moveNotation = new StringBuilder();
            string ep = "";
            //Get move notation
            if (pt.type != pieceType.PAWN)
            {
                moveNotation.Append("QKRNB"[(int)pt.type]);
            }
            else
            {
                Point otherPt = new Point(dest.X + travel.X, origin.Y);
                if (travel.X != 0 && MoveCalculation.validPoint(otherPt))
                {
                    Piece? otherPawnMaybe = board[otherPt.X, otherPt.Y];
                    if (otherPawnMaybe != null && otherPawnMaybe.type == pieceType.PAWN)
                    {
                        moveNotation.Append("abcdefgh"[origin.X]);
                    }
                }
            }


            // Captured pieces cannot move.
            if (replaced != null)
            {
                moveNotation.Append("x");
                if (replaced.team == Team.BLACK)
                {
                    blackPieces.Remove(replaced);
                }
                else
                    whitePieces.Remove(replaced);
            }

            moveNotation.Append("abcdefgh"[dest.X]);
            moveNotation.Append("87654321"[dest.Y]);

            // Castling check
            if (pt.type == pieceType.KING)
            {
                King pc = (King)pt;
                pc.canCastle = false;
                // If it's castling
                if (Math.Abs(origin.X - dest.X) == 2)
                {
                    moveNotation.Clear();
                    moveNotation.Append("O-O");
                    // Move the rook.
                    int rookXpos = dest.X > 3 ? 7 : 0;
                    int offset = dest.X > 3 ? -1 : 1;
                    if (offset == -1)
                        moveNotation.Append("-O");
                    Rook rook = (Rook)board[rookXpos, dest.Y];
                    Point rOrigin = rook.position;
                    Point rDest = new Point(dest.X + offset, dest.Y);
                    board[rOrigin.X, rOrigin.Y] = null;
                    board[rDest.X, rDest.Y] = rook;
                }
            }


            // Special case for pawn moving.
            if (pt.type == pieceType.PAWN)
            {
                Pawn movePc = (Pawn)pt;

                // Check if the move was e.p.
                if (replaced == null)
                {
                    int x = Math.Abs(origin.X - dest.X);
                    if (x == 1)
                    {
                        replaced = board[dest.X, origin.Y];
                        board[dest.X, origin.Y] = null;
                        moveNotation.Append(" e.p.");
                    }
                }

                // Ensure double move is only once.
                movePc.canDoubleMove = false;
                // En passant check
                if (Math.Abs(origin.Y - dest.Y) == 2)
                    movePc.canBeEP = true;
                else
                    movePc.canBeEP = false;
                // Promotion check
                int final = movePc.team == Team.BLACK ? 7 : 0;
                pawnProm = movePc.position.Y == final;
            }

            // Set other piece's variables accordingly.
            if (pt.type == pieceType.ROOK)
            {
                Rook pc = (Rook)pt;
                pc.canCastle = false;
            }
            
            return moveNotation;
        }

        // Perform a piece promotion after user has made selection
        internal static void promotePiece(Piece pc, pieceType type, Team turn)
        {
            Point pos = pc.position;
            Team team = pc.team;
            Piece replacement;
            board[pos.X, pos.Y] = null;
            switch(type)
            {
                case pieceType.QUEEN:
                    replacement = new Queen(team);
                    break;
                case pieceType.KNIGHT:
                    replacement = new Queen(team);
                    break;
                case pieceType.ROOK:
                    replacement = new Rook(team);
                    break;
                case pieceType.BISHOP:
                    replacement = new Bishop(team);
                    break;
                default:
                    throw new Exception();
            }
            board[pos.X, pos.Y] = replacement;
            if (pc.team == Team.BLACK)
            {
                blackPieces.Remove(pc);
                blackPieces.Add(replacement);
            }
            else
            {
                whitePieces.Remove(pc);
                whitePieces.Add(replacement);
            }
            // Add list of other team's moves.
            MoveCalculation.otherMoves.Clear();
            List<Piece> otherPcs = turn == Team.BLACK ? whitePieces : blackPieces;
            foreach (Piece otherPc in otherPcs)
            {
                MoveCalculation.otherMoves.Add(otherPc, otherPc.calculateMoves(false));
            }
        }
    }
}
