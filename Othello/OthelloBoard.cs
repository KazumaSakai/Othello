using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Othello
{
    /// <summary>
    /// オセロのゲームを実際に行うボードのオブジェクト
    /// </summary>
    public class OthelloBoard : IDebugOutput, IFormPanel
    {
        //
        //  Variable
        //
        /// <summary>
        /// マスの状態
        /// </summary>
        public enum SquareState
        {
            None,
            Black,
            White
        }
        /// <summary>
        /// ボードにあるマスのデータ配列
        /// </summary>
        private SquareState[,] boardSquaresData;

        /// <summary>
        /// 方向
        /// </summary>
        private enum Direction
        {
            Up,
            RightUp,
            Right,
            RightDown,
            Down,
            LeftDown,
            Left,
            LeftUp
        }
        /// <summary>
        /// 方向のベクトル
        /// </summary>
        public Vector2[] deltaDirection = { new Vector2(0, -1), new Vector2(1, -1), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), new Vector2(-1, 1), new Vector2(-1, 0), new Vector2(-1, -1) };

        /// <summary>
        /// ゲームが既に終了している
        /// </summary>
        public bool EndGame;

        /// <summary>
        /// 次のプレイヤーID
        /// </summary>
        private int nextPlayerId;
        /// <summary>
        /// ボードの大きさ
        /// </summary>
        private int boardSize;
        /// <summary>
        /// プレイヤーの数
        /// </summary>
        private int playerNumber;

        /// <summary>
        /// 石を置ける場所
        /// </summary>
        private List<(Vector2, int)> CanPutPointList;

        //
        //  Method
        //
        /// <summary>
        /// コンストラクタ
        /// <para>オセロのボードを作成する</para>
        /// </summary>
        /// <param name="boardSize">ボードの大きさ</param>
        /// <param name="playerNumber">プレイヤーの数</param>
        public OthelloBoard(int boardSize = 8, int playerNumber = 2)
        {
            if (boardSize <= 1) boardSize = 2;
            if (boardSize % 2 != 0) boardSize += 1;

            this.boardSquaresData = new SquareState[boardSize, boardSize];
            this.playerNumber = playerNumber;
            this.boardSize = boardSize;
            this.EndGame = false;
            this.CanPutPointList = new List<(Vector2, int)>(boardSize * boardSize);

            Initialization();
        }
        
        /// <summary>
        /// ボードを初期化する
        /// </summary>
        public void Initialization()
        {
            int centerPosition = boardSize / 2;

            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    boardSquaresData[x, y] = SquareState.None;
                }
            }
            boardSquaresData[centerPosition - 1, centerPosition - 1] = (SquareState)1;
            boardSquaresData[centerPosition, centerPosition] = (SquareState)1;
            boardSquaresData[centerPosition -1, centerPosition] = (SquareState)2;
            boardSquaresData[centerPosition, centerPosition - 1] = (SquareState)2;

            nextPlayerId = 0;

            EndGame = false;
            UpdateBoardPanel();
            FindCanPutPoint();
        }

        /// <summary>
        /// オセロの石を置くことができるか
        /// </summary>
        /// <param name="x">X座標 左右</param>
        /// <param name="y">Y座標 上下</param>
        /// <returns>置くことができるなら true 、不可能ならば false</returns>
        public bool CanPutDisc(Vector2 position, int playerId = -1)
        {
            if (playerId < 0) playerId = nextPlayerId;
            bool outSideBoard = this.OutSideBoard(position);
            bool notEmptySquare = (boardSquaresData[position.x, position.y] != SquareState.None);

            if (outSideBoard || notEmptySquare) return false;

            //  ８方位すべてを調べる
            for (int i = 0; i < 8; i++)
            {
                if (InversLength(position, (Direction)(i), playerId) != 0) return true;
            }

            return false;
        }
        /// <summary>
        /// オセロの石を置くことができるか
        /// </summary>
        /// <param name="x">X座標 左右</param>
        /// <param name="y">Y座標 上下</param>
        /// <param name="playerId">プレイヤーのID</param>
        /// <returns>置くことができるなら true 、不可能ならば false</returns>
        public bool CanPutDisc(int x, int y, int playerId = -1)
        {
            return CanPutDisc(new Vector2(x, y));
        }

        /// <summary>
        /// オセロの石をボード上に置く
        /// <para>置くことに失敗したら false を返す</para>
        /// </summary>
        /// <param name="position">座標</param>
        /// <param name="playerId">プレイヤーのID</param>
        /// <returns>置くことができたか</returns>
        public bool PutDisc(Vector2 position, int playerId = -1)
        {
            if (EndGame)
            {
                NextGame();
                return false;
            }
            if (playerId < 0 || playerId >= playerNumber) playerId = nextPlayerId;
            if (!CanPutDisc(position, playerId)) return false;

            boardSquaresData[position.x, position.y] = (SquareState)(playerId + 1);

            for (int i = 0; i < 8; i++)
            {
                InversDisc(position, (Direction)(i), playerId);
            }

            for (int i = 0; i < 2; i++)
            {
                NextPlayer(playerId);
                FindCanPutPoint();
                if (CanPutPointList.Count > 0)
                {
                    if(playerId == 0)
                    {
                        Vector2 maxPos = CanPutPointList[0].Item1;
                        int maxpoint = int.MinValue;
                        foreach ((Vector2, int) point in CanPutPointList)
                        {
                            if (maxpoint < point.Item2)
                            {
                                maxPos = point.Item1;
                                maxpoint = point.Item2;
                            }
                        }
                        PutDisc(maxPos);
                        return true;
                    }

                    UpdateBoardPanel();
                    return true;
                }
            }

            EndGame = true;
            UpdateBoardPanel();
            return true;
        }
        /// <summary>
        /// オセロの石をボード上に置く
        /// <para>置くことに失敗したら false を返す</para>
        /// </summary>
        /// <param name="x">X座標 左右</param>
        /// <param name="y">Y座標 上下</param>
        /// <returns>置くことができたか</returns>
        public bool PutDisc(int x, int y, int playerId = -1)
        {
            return PutDisc(new Vector2(x, y));
        }

        /// <summary>
        /// 次のプレイヤーへパス
        /// </summary>
        public void NextPlayer()
        {
            nextPlayerId += 1;
            if (nextPlayerId >= playerNumber) nextPlayerId = 0;
        }
        /// <summary>
        /// 次のプレイヤーへパス
        /// </summary>
        /// <param name="playerId">現在のプレイヤーId</param>
        private void NextPlayer(int playerId)
        {
            nextPlayerId = playerId + 1;
            if (nextPlayerId >= playerNumber) nextPlayerId = 0;
        }

        /// <summary>
        /// 指定の座標がボード内であるか
        /// </summary>
        /// <param name="position">座標</param>
        /// <returns>座標ないである true</returns>
        private bool OutSideBoard(Vector2 position)
        {
            return OutSideBoard(position.x, position.y);
        }
        /// <summary>
        /// 指定の座標がボード内であるか
        /// </summary>
        /// <param name="x">X座標 左右</param>
        /// <param name="y">Y座標 上下</param>
        /// <returns>座標ないである true</returns>
        private bool OutSideBoard(int x, int y)
        {
            return (x < 0 || y < 0 || x >= boardSize || y >= boardSize);
        }

        /// <summary>
        /// 反転することができる長さを返します
        /// </summary>
        /// <param name="x">X座標 左右</param>
        /// <param name="y">Y座標 上下</param>
        /// <param name="playerId">石を置くプレイヤー</param>
        /// <param name="direction">方向</param>
        /// <returns>消せる長さ</returns>
        private int InversLength(Vector2 position, Direction direction, int playerId = -1)
        {
            if (playerId < 0 || playerId >= playerNumber) playerId = nextPlayerId;

            for (int i = 0; true; i++)
            {
                position.x += deltaDirection[(int)direction].x;
                position.y += deltaDirection[(int)direction].y;

                if (OutSideBoard(position) || (boardSquaresData[position.x, position.y] == SquareState.None)) return 0;

                bool isMyDisc = (boardSquaresData[position.x, position.y] == (SquareState)(playerId + 1));

                if (isMyDisc)
                {
                    return i;
                }
            }
        }
        /// <summary>
        /// 石を置いて、相手の石を反転させます
        /// </summary>
        /// <param name="position">座標</param>
        /// <param name="playerId">プレイヤーID</param>
        /// <param name="length">反転させる長さ</param>
        /// <param name="direction">方向</param>
        private void InversDisc(Vector2 position, Direction direction, int playerId = -1)
        {
            if (playerId < 0 || playerId >= playerNumber) playerId = nextPlayerId;

            int l = InversLength(position, direction, playerId);
            for (int i = 0; i < l; i++)
            {
                position.x += deltaDirection[(int)direction].x;
                position.y += deltaDirection[(int)direction].y;

                boardSquaresData[position.x, position.y] = (SquareState)(playerId + 1);
            }
        }

        /// <summary>
        /// 次のゲームを開始する
        /// </summary>
        public void NextGame()
        {
            (int, int) result = CountDisc();

            Initialization();
        }

        /// <summary>
        /// おける位置を探す
        /// </summary>
        private void FindCanPutPoint()
        {
            if (CanPutPointList.Count > 0) CanPutPointList.Clear();
            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    if (CanPutDisc(x, y))
                    {
                        int p = 1;
                        if ((x == 0 || x == 7) && (y == 0 || y == 7)) p = 100;
                        if ((x == 1 || x == 6) && (y == 0 || y == 7)) p = -100;
                        if ((x == 0 || x == 7) && (y == 1 || y == 6)) p = -100;
                        if ((x == 1 || x == 6) && (y == 1 || y == 6)) p = -100;
                        CanPutPointList.Add((new Vector2(x, y), p));
                    }
                }
            }
        }

        /// <summary>
        /// 石の数を数える
        /// </summary>
        /// <returns>黒の数、　白の数</returns>
        private (int, int) CountDisc()
        {
            int black = 0, white = 0;
            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    switch (boardSquaresData[x, y])
                    {
                        case SquareState.Black:
                            black++;
                            break;

                        case SquareState.White:
                            white++;
                            break;
                    }
                }
            }
            return (black, white);
        }


        //
        //  IDebugOutput
        //
        /// <summary>
        /// デバッグ用、現在の状況を出力する
        /// </summary>
        public void DebugOutput()
        {
            Console.Clear();

            Console.WriteLine("NextPlayer: " + ((nextPlayerId == 0) ? "●\n" : "〇\n"));

            StringBuilder sb = new StringBuilder();
            for (int y = 0; y <= boardSize; y++)
            {
                for (int x = 0; x <= boardSize; x++)
                {
                    if (y == 0)
                    {
                        sb.Append(((char)('０' + x)).ToString());
                        continue;
                    }
                    if (x == 0)
                    {
                        sb.Append(((char)('０' + y)).ToString());
                        continue;
                    }

                    switch (boardSquaresData[x - 1, y - 1])
                    {
                        case SquareState.None:
                            if (CanPutDisc(x - 1, y - 1))
                            {
                                sb.Append("・");
                            }
                            else
                            {
                                sb.Append("　");
                            }
                            break;

                        case SquareState.White:
                            sb.Append("〇");
                            break;

                        case SquareState.Black:
                            sb.Append("●");
                            break;
                    }
                }
                Console.WriteLine(sb.ToString());
                sb.Clear();
            }
        }


        //
        //  IFormPanel
        //
        /// <summary>
        /// フォームに表示する用のパネル
        /// </summary>
        public Panel formPanel
        {
            get
            {
                if (boardPanel == null)
                {
                    CreateFormPanel();
                }
                return boardPanel;
            }
        }
        /// <summary>
        /// フォームに表示するボードのパネル
        /// </summary>
        private Panel boardPanel;
        /// <summary>
        /// マス（石）のパネル
        /// </summary>
        private Panel[,] squarePanels;
        /// <summary>
        /// マス(石)の画像 データ配列
        /// </summary>
        private Image[] squareImage = new Image[] { null, Properties.Resources.Black_Disc, Properties.Resources.White_Disc };
        private Image[] squareTransparentImage = new Image[] { Properties.Resources.Black_Disc_Transparent, Properties.Resources.White_Disc_Transparent };
        private Label resultLabel;

        /// <summary>
        /// フォーム用のパネルを作成する
        /// </summary>
        private void CreateFormPanel()
        {
            boardPanel = new Panel();
            boardPanel.SuspendLayout();
            boardPanel.Location = new Point(20, 20);
            boardPanel.Margin = new Padding(1);
            boardPanel.Size = new Size(340, 370);
            boardPanel.TabIndex = 0;
        
            squarePanels = new Panel[boardSize, boardSize];
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    squarePanels[x, y] = new Panel();
                    squarePanels[x, y].BackColor = Color.Green;

                    squarePanels[x, y].BackgroundImage = squareImage[(int)boardSquaresData[x,y]];
                    squarePanels[x, y].BackgroundImageLayout = ImageLayout.Center;
                    squarePanels[x, y].BorderStyle = BorderStyle.FixedSingle;
                    squarePanels[x, y].Location = new Point(1 + (41 * x), 1 + (41 * y));
                    squarePanels[x, y].Margin = new Padding(1);
                    squarePanels[x, y].Size = new Size(40, 40);
                    squarePanels[x, y].TabIndex = 0;
                    squarePanels[x, y].Click += new EventHandler(ClickSquare);
                    squarePanels[x, y].Name = sb.Append(x).Append(" ").Append(y).ToString();
                    boardPanel.Controls.Add(squarePanels[x, y]);

                    sb.Clear();
                }
            }

            resultLabel = new Label();
            resultLabel.Location = new Point(10, 340);
            resultLabel.TextAlign = ContentAlignment.MiddleCenter;
            resultLabel.Size = new Size(320, 20);
            resultLabel.TabIndex = 0;
            resultLabel.Font = new Font(resultLabel.Font.OriginalFontName, 12.0f);
            boardPanel.Controls.Add(resultLabel);

            boardPanel.ResumeLayout(false);

            UpdateBoardPanel();
        }

        /// <summary>
        /// マス目をクリックしたときのイベント
        /// </summary>
        /// <param name="sender">送り主</param>
        /// <param name="e">EventArgs</param>
        private void ClickSquare(object sender, EventArgs e)
        {
            string[] xy = (sender as Panel).Name.Split(' ');
            int x = int.Parse(xy[0]);
            int y = int.Parse(xy[1]);
            PutDisc(x, y);
        }

        /// <summary>
        /// ボードのパネルを更新する
        /// </summary>
        private void UpdateBoardPanel()
        {
            if (boardPanel == null) return;

            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    if (boardSquaresData[x, y] == SquareState.None && CanPutDisc(x, y))
                    {
                        squarePanels[x, y].BackgroundImage = squareTransparentImage[nextPlayerId];
                    }
                    else
                    {
                        squarePanels[x, y].BackgroundImage = squareImage[(int)boardSquaresData[x, y]];
                    }
                }
            }

            StringBuilder sb = new StringBuilder();

            (int, int) result = CountDisc();

            string winner = (result.Item1 == result.Item2) ? "引き分け　" : (result.Item1 > result.Item2) ? "黒の勝ち　" : "白の勝ち　";
            resultLabel.Text = sb.Append(EndGame ? winner : "").Append("黒: ").Append(result.Item1).Append(", 白: ").Append(result.Item2).ToString();
        }
    }
}
