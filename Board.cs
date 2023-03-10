using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Chess3
{
    public partial class Board : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        private void Drag(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        // A static 2D array of buttons, representing the board.
        // Not to be modified in calculation, merely used for visual sake.
        public static Button[,] boardButtons = new Button[8, 8];
        private static Bitmap pieceTexture = Properties.Resources.pieces;
        private static Piece? selectedPiece;
        private Team turn = Team.WHITE;
        // Constructor
        public Board()
        {
            InitializeComponent();
            bishopProm.Click += promotionClick;
            knightProm.Click += promotionClick;
            queenProm.Click += promotionClick;
            rookProm.Click += promotionClick;
        }

        // When the board loads, set up the buttons.
        private void Board_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    // Get every button in the panel.
                    Button btn = (Button)boardPanel.GetChildAtPoint(new Point(i * 80, j * 80));
                    // Configure text + colour
                    btn.Text = "";
                    btn.BackgroundImageLayout = ImageLayout.Center;
                    btn.FlatAppearance.BorderSize = 0;
                    if ((i+j) % 2 == 0)
                    {
                        btn.BackColor = Color.PapayaWhip;
                    }
                    else
                    {
                        btn.BackColor = Color.DarkOliveGreen;
                    }
                    btn.FlatAppearance.MouseOverBackColor = btn.BackColor;
                    // Add delegate for the click event
                    btn.Click += boardButtonClick;
                    // Add to the static array
                    boardButtons[i, j] = btn;
                }
            }
            GameHandler.setupGame();
            updateBoard();
        }

        private void displayMoves(Piece pc)
        {
            List<Point> moves = pc.calculateMoves(true);
            foreach (Point pt in moves)
            {
                if (GameHandler.board[pt.X, pt.Y] == null)
                    boardButtons[pt.X, pt.Y].BackgroundImage = Properties.Resources.moveCircle;
                else
                    boardButtons[pt.X, pt.Y].BackgroundImage = Properties.Resources.captureCircle;
            }
        }

        private static StringBuilder tempNote;

        // Function called whenever a button corresponding to a piece on the board is clicked.
        private void boardButtonClick(object? sender, EventArgs e)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    boardButtons[i, j].BackgroundImage = null;
                }
            }    
            if (sender == null) throw new Exception("Null button");
            Button btn = (Button)sender;
            Point pos = getPos(btn);
            if (selectedPiece == null)
            {
                Piece? pc = GameHandler.board[pos.X, pos.Y];
                if (pc == null
                    || pc.team != turn) return;
                selectedPiece = pc;

                displayMoves(pc);
            }
            else
            {
                Piece? pc = GameHandler.board[pos.X, pos.Y];
                if (selectedPiece == pc)
                {
                    selectedPiece = null;
                    return;
                }
                else if(pc != null && selectedPiece.team == pc.team)
                {
                    selectedPiece = pc;
                    displayMoves(pc);
                }

                List<Point> pieceMoves = selectedPiece.calculateMoves(true);
                if (pieceMoves.Contains(pos))
                {
                    StringBuilder notation = GameHandler.movePiece(selectedPiece, pos);

                    turn = turn == Team.BLACK ? Team.WHITE : Team.BLACK;




                    // Promotion check
                    if (GameHandler.pawnProm)
                    {
                        GameHandler.pawnProm = false;
                        tempNote = notation;
                        pawnPromotePanel();
                        return;
                    }
                    selectedPiece = null;
                    // Add list of other team's moves.
                    MoveCalculation.otherMoves.Clear();
                    List<Piece> otherPcs = turn == Team.BLACK ? GameHandler.whitePieces : GameHandler.blackPieces;
                    foreach (Piece otherPc in otherPcs)
                    {
                        MoveCalculation.otherMoves.Add(otherPc, otherPc.calculateMoves(false));
                    }
                    updateBoard();
                    // Check if valid moves
                    bool gameOver = true;
                    List<Piece> ourPcs = turn == Team.BLACK ? GameHandler.blackPieces : GameHandler.whitePieces;
                    foreach(Piece movePc in ourPcs)
                    {
                        if (movePc.calculateMoves(true).Count != 0)
                            gameOver = false;
                    }
                    //No legal moves
                    if (gameOver) 
                        setBoard(false);
                    bool kingAttacked = false;
                    Point kingPos = ourPcs.Where(x => x.type == pieceType.KING).First().position;
                    foreach(Point move in MoveCalculation.otherMoves.Values.SelectMany(x => x))
                    {
                        if (move == kingPos)
                        {
                            kingAttacked = true;
                            break;
                        }
                    }
                    string gameText = "";
                    if (kingAttacked && gameOver)
                    {
                        //Checkmate
                        notation.Append("#");
                        string winner = turn == Team.BLACK ? "White" : "Black";
                        gameEndText.Text = $"Checkmate! {winner} wins!";
                        gameOverPanel.Visible = true;
                        gameText = winner == "White" ? "1-0" : "0-1";
                    }
                    else if (gameOver)
                    {
                        //Stalemate
                        notation.Append("$");
                        gameEndText.Text = "Stalemate!";
                        gameOverPanel.Visible = true;
                        gameText = "½-½";
                    }
                    else if (kingAttacked)
                        notation.Append("+");
                    string s = notation.ToString();
                    addNote(s);
                    if (gameText != "")
                        addNote(gameText);
                }

            }

        }

        private void checkGameEnded(List<Piece> ourPcs, StringBuilder notation, bool replace)
        {
            bool gameOver = true;
            bool kingAttacked = false;
            foreach (Piece movePc in ourPcs)
            {
                if (movePc.calculateMoves(true).Count != 0)
                    gameOver = false;
            }
            Point kingPos = ourPcs.Where(x => x.type == pieceType.KING).First().position;
            foreach (Point move in MoveCalculation.otherMoves.Values.SelectMany(x => x))
            {
                if (move == kingPos)
                {
                    kingAttacked = true;
                    break;
                }
            }

            string gameText = "";
            if (kingAttacked && gameOver)
            {
                //Checkmate
                notation.Append("#");
                string winner = turn == Team.BLACK ? "White" : "Black";
                gameEndText.Text = $"Checkmate! {winner} wins!";
                gameOverPanel.Visible = true;
                gameText = winner == "White" ? "1-0" : "0-1";
            }
            else if (gameOver)
            {
                //Stalemate
                notation.Append("$");
                gameEndText.Text = "Stalemate!";
                gameOverPanel.Visible = true;
                gameText = "½-½";
            }
            else if (kingAttacked)
                notation.Append("+");
            string s = notation.ToString();
            addNote(s);
            if (gameText != "")
                addNote(gameText);
        }
    

        // Top-right button, to exit the program.
        private void exitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Get a point from a position
        private Point getPos(Button btn)
        {
            Point pos = btn.Location;
            return new Point(pos.X / 80, pos.Y / 80);
        }

        // Pawn promotion panel
        internal void pawnPromotePanel()
        {
            setBoard(false);
            Team promTeam = turn == Team.BLACK ? Team.WHITE : Team.BLACK;
            bishopProm.BackgroundImage = getPieceImage(promTeam, pieceType.BISHOP);
            queenProm.BackgroundImage = getPieceImage(promTeam, pieceType.QUEEN);
            knightProm.BackgroundImage = getPieceImage(promTeam, pieceType.KNIGHT);
            rookProm.BackgroundImage = getPieceImage(promTeam, pieceType.ROOK);

            promotionPanel.Visible = true;
        }

        internal void promotionClick(object sender, EventArgs e)
        {
            Button clicked = (Button)sender;
            pieceType type = (pieceType)clicked.Tag;
            GameHandler.promotePiece(selectedPiece, type, turn);
            moveBox.AppendText($"={"QKRNB"[(int)type]}");
            // Return to game
            promotionPanel.Visible = false;
            selectedPiece = null;
            updateBoard();
            setBoard(true);
        }



        // Static funcs to update the board

        internal void addNote(string note)
        {
            moveBox.AppendText(note);
            if (turn == Team.WHITE)
            {
                moveBox.AppendText(Environment.NewLine);
                moveBox.AppendText($"{moveBox.Lines.Length}. ");
            }
            else
                moveBox.AppendText(" ");
        }

        internal Bitmap getPieceImage(Team team, pieceType type)
        {
            int X = (int)type;
            int Y = team == Team.BLACK ? 0 : 1;
            Rectangle res = new Rectangle(X * 60, Y * 60, 60, 60);
            return pieceTexture.Clone(res, pieceTexture.PixelFormat);
        }
        private Bitmap getPieceImage(Button btn)
        {
            if (btn.Tag == null) return new Bitmap(60,60);

            string tag = (string)btn.Tag;
            char team = tag[0];
            char type = tag[1];
            int x = int.Parse(type.ToString());
            int y = team == 'B' ? 0 : 1;
            Rectangle res = new Rectangle(x * 60, y * 60, 60, 60);
            return pieceTexture.Clone(res, pieceTexture.PixelFormat);
        }
        private void refreshVisualBoard()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Button btn = (Button)boardPanel.GetChildAtPoint(new Point(i * 80, j * 80));
                    btn = boardButtons[i, j];
                    btn.Image = getPieceImage(btn);
                }
            }
        }
        internal void updateBoard()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Piece pc = GameHandler.board[i, j];
                    if (pc == null)
                    {
                        boardButtons[i, j].Tag = null;
                        continue;
                    }
                    string tm = pc.team.ToString();
                    int typ = (int)pc.type;
                    boardButtons[i, j].Tag = tm[0].ToString() + typ.ToString();
                    
                }
            }
            refreshVisualBoard();
        }

        internal void setBoard(bool enabled)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    boardButtons[i, j].Enabled = enabled;
                }
            }
        }

        private void replayButton_Click(object sender, EventArgs e)
        {
            turn = Team.WHITE;
            gameOverPanel.Visible = false;
            selectedPiece = null;
            moveBox.Clear();

            GameHandler.setupGame();
            setBoard(true);
            updateBoard();
        }
    }
}
