using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess3
{

    // Inherited class for pawns
    internal class Pawn : Piece
    {
        internal bool canDoubleMove;
        internal bool canPromote;
        internal bool canBeEP;

        public Pawn(Team team) : base(team)
        {
            type = pieceType.PAWN;
            canDoubleMove = true;
            canBeEP = false;
            canPromote = false;
        }

        internal override List<Point> calculateMoves(bool sanitise)
        {
            List<Point> moves = new List<Point>();
            int yOffset = team == Team.BLACK ? 1 : -1;
            // Moving forward
            Point forwardPoint = new Point(position.X, position.Y + yOffset);
            if (MoveCalculation.validPoint(forwardPoint))
            {
                if (GameHandler.board[forwardPoint.X, forwardPoint.Y] == null)
                    moves.Add(forwardPoint);
            }

            // Double move
            if (canDoubleMove)
            {
                Point fwd2Point = new Point(position.X, position.Y + yOffset * 2);
                if (MoveCalculation.validPoint(fwd2Point))
                {
                    if (GameHandler.board[fwd2Point.X, fwd2Point.Y] == null)
                        moves.Add(fwd2Point);
                }
            }
            // Moving diagonal
            for(int i = -1; i <= 1; i += 2)
            {
                // Conventional attacking
                Point attackPoint = new Point(position.X + i, position.Y + yOffset);
                if (!MoveCalculation.validPoint(attackPoint)) continue;
                Piece pc = GameHandler.board[attackPoint.X, attackPoint.Y];
                if (pc != null
                    && pc.team != team)
                    moves.Add(attackPoint);

                // En passant
                
                Piece? epPiece = GameHandler.board[attackPoint.X, position.Y];
                
                if (epPiece != null && epPiece.type == pieceType.PAWN && epPiece.team != team && ((Pawn)epPiece).canBeEP)
                    moves.Add(attackPoint);
            }


            return filterMoves(moves, sanitise);
        }
    }

    internal class Rook : Piece
    {
        internal bool canCastle;
        public Rook(Team team) : base(team)
        {
            type = pieceType.ROOK;
            canCastle = true;
        }
        internal override List<Point> calculateMoves(bool sanitise)
        {
            List<Point> unsanitisedMoves = MoveCalculation.calculateStraightLines(GameHandler.board, this, 8);

            return filterMoves(unsanitisedMoves, sanitise);
        }
    }

    internal class Bishop : Piece
    {
        public Bishop(Team team) : base(team)
        {
            type = pieceType.BISHOP;
        }

        internal override List<Point> calculateMoves(bool sanitise)
        {
            List<Point> unsanitisedMoves = MoveCalculation.calculateDiagonals(GameHandler.board, this, 8);

            return filterMoves(unsanitisedMoves, sanitise);
        }
    }

    internal class Knight : Piece
    {
        public Knight(Team team) : base(team)
        {
            type = pieceType.KNIGHT;
        }

        internal override List<Point> calculateMoves(bool sanitise)
        {
            List<Point> unsanitisedMoves = new List<Point>();

            // Use loops to find all knight moves.
            for (int i = -2; i < 3; i += 4)
            {
                for (int j = -1; j < 2; j += 2)
                {
                    Point a = new Point(position.X + i, position.Y + j);
                    Point b = new Point(position.X + j, position.Y + i);
                    if (MoveCalculation.validPoint(a))
                        unsanitisedMoves.Add(a);
                    if (MoveCalculation.validPoint(b))
                        unsanitisedMoves.Add(b);
                }
            }
            
            return filterMoves(unsanitisedMoves, sanitise);
        }
    }

    internal class Queen : Piece
    {
        public Queen(Team team) : base(team)
        {
            type = pieceType.QUEEN;
        }

        internal override List<Point> calculateMoves(bool sanitise)
        {
            List<Point> unsanitisedMoves = MoveCalculation.calculateDiagonals(GameHandler.board, this, 8)
                .Concat(MoveCalculation.calculateStraightLines(GameHandler.board, this, 8)).ToList();

            return filterMoves(unsanitisedMoves, sanitise);
        }
    }

    internal class King : Piece
    {
        internal bool canCastle;

        public King(Team team) : base(team)
        {
            type = pieceType.KING;
            canCastle = true;
        }

        internal override List<Point> calculateMoves(bool sanitise)
        {
            List<Point> unsanitisedMoves = MoveCalculation.calculateDiagonals(GameHandler.board, this, 1)
                .Concat(MoveCalculation.calculateStraightLines(GameHandler.board, this, 1)).ToList();

            //Castle check
            if (canCastle)
            {
                for(int offset = 1; offset >= -1; offset -= 2)
                {
                    for(int i = offset; i > -5 && i < 5; i += offset)
                    {
                        Point newPos = new Point(position.X + i, position.Y);
                        if (!MoveCalculation.validPoint(newPos))
                            break;
                        Piece? PieceCheck = GameHandler.board[newPos.X, newPos.Y];
                        if (PieceCheck == null)
                            continue;
                        if (PieceCheck.type != pieceType.ROOK)
                            break;
                        Rook pc = (Rook)PieceCheck;
                        if (!pc.canCastle) break;
                        unsanitisedMoves.Add(new Point(position.X + 2 * offset, position.Y));
                    }
                }
            }
            return filterMoves(unsanitisedMoves, sanitise);
        }
    }


}
