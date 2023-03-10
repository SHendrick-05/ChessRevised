using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess3
{
    enum Direction
    {
        NONE,
        UP,
        DOWN,
        LEFT,
        RIGHT
    }
    internal static class MoveCalculation
    {
        internal static Dictionary<Piece, List<Point>> otherMoves = new Dictionary<Piece, List<Point>>();
        internal static bool validPoint(Point pt)
            => !(pt.X > 7 || pt.X < 0 || pt.Y > 7 || pt.Y < 0);
        private static List<Point> calcLine(Piece?[,] board, Piece movingPiece, int xOffset, int yOffset, int length)
        {
            List<Point> result = new List<Point>();
            Point prevPt = movingPiece.position;
            for (int i = 0; i < length; i++)
            {
                Point newPoint = new Point(prevPt.X + xOffset, prevPt.Y + yOffset);
                if (!validPoint(newPoint)) break;
                Piece pc = board[newPoint.X, newPoint.Y];
                if (pc == null)
                {
                    result.Add(newPoint);
                    prevPt = newPoint;
                }
                else if (pc.team != movingPiece.team)
                {
                    result.Add(newPoint);
                    break;
                }
                else break;
            }
            return result;
        }

        internal static List<Point> calculateStraightLines(Piece?[,] board, Piece movingPiece, int length)
        {
            List<Point> left = calcLine(board, movingPiece, -1, 0, length);
            List<Point> right = calcLine(board, movingPiece, 1, 0, length);
            List<Point> up = calcLine(board, movingPiece, 0, -1, length);
            List<Point> down = calcLine(board, movingPiece, 0, 1, length);
            return left.Concat(right.Concat(up.Concat(down))).ToList();
        }

        internal static List<Point> calculateDiagonals(Piece?[,] board, Piece movingPiece, int length)
        {
            List<Point> LU = calcLine(board, movingPiece, -1, -1, length);
            List<Point> LD = calcLine(board, movingPiece, -1, 1, length);
            List<Point> RU = calcLine(board, movingPiece, 1, -1, length);
            List<Point> RD = calcLine(board, movingPiece, 1, 1, length);
            return LU.Concat(LD.Concat(RU.Concat(RD))).ToList();
        }

        internal static List<Point> sanitiseMoves(Piece?[,] board, Piece movingPiece, List<Point> moves)
        {

            // Make sure no friendly fire happens
            List<Point> filteredMoves = moves.Where(move => board[move.X, move.Y] == null
                    || board[move.X, move.Y] != null && board[move.X, move.Y].team != movingPiece.team).ToList();

            //Sanitise for check
            List<Point> result = new List<Point>();
            Team team = movingPiece.team;
            List<Piece> teamPcs = team == Team.BLACK ? GameHandler.blackPieces : GameHandler.whitePieces;
            Point _kingPos = teamPcs.Where(x => x.type == pieceType.KING).FirstOrDefault().position;

            Piece pc = movingPiece;
            Point origin = pc.position;
            List<Piece> attackingPieces = otherMoves.Keys.ToList();
                
            bool isNull = false;
            board[origin.X, origin.Y] = null;
            foreach(Point dest in filteredMoves)
            {
                Piece tempPc = board[dest.X, dest.Y];
                bool validMove = true;
                //Simulate the move
                if (tempPc == null)
                {
                    isNull = true;
                    board[dest.X, dest.Y] = pc;
                }
                else
                {
                    isNull = false;
                    board[dest.X, dest.Y] = pc;
                }
                // Check the king is not under attack
                foreach(Piece attackingPiece in attackingPieces)
                {
                    // If the piece will be captured anyway, no point calculating moves
                    if (dest == attackingPiece.position) 
                        continue;
                    // Ensure the piece is not attacking the king
                    List<Point> attackMoves = attackingPiece.calculateMoves(false);
                    Point kingPos = pc.type == pieceType.KING ? dest : _kingPos;
                    if (attackMoves.Contains(kingPos))
                        validMove = false;
                            
                }
                if (validMove)
                    result.Add(dest);
                if (isNull)
                {
                    board[dest.X, dest.Y] = null;
                }
                else
                {
                    board[dest.X, dest.Y] = tempPc;
                }
            }
            // Return the piece to original pos.
            board[origin.X, origin.Y] = pc;
            // Return the filtered moves
            return result;
        }
        internal static List<Point> checkFilterMoves(Piece?[,] board, Piece movingPiece, List<Point> moves)
        {
            throw new NotImplementedException();
        }
    }
}
