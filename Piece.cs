using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess3
{
    // Enum type referring to the possible types of pieces.
    enum pieceType
    {
         QUEEN,
         KING,
         ROOK,
         KNIGHT,
         BISHOP,
         PAWN
    }
    // Enum to distinguish black and white pieces.
    enum Team
    {
        BLACK,
        WHITE
    }
    // Base class of piece
    // Only used for inheritance by child classes representing different pieces.
    internal abstract class Piece
    {
        // Fields to use
        internal Point position
        {
            get
            {
                return GameHandler.getPiecePos(this);
            }
            set { position = value; }
        }
        internal pieceType type;
        internal Team team;
        // The class to be overridden by each inherited class.
        internal Piece(Team team)
        {
            this.team = team;
        }

        internal abstract List<Point> calculateMoves(bool sanitise);

        internal List<Point> filterMoves(List<Point> moves, bool sanitise)
        {
            if (sanitise)
            {
                List<Point> sanitisedMoves = MoveCalculation.sanitiseMoves(GameHandler.board, this, moves);
                return sanitisedMoves;
            }
            return moves;
        }
    }
}
