using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Othello
{
    /// <summary>
    /// オセロのゲームを実際に行うボードのオブジェクト
    /// </summary>
    public class OthelloBoard
    {

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

            Initialization();
        }
        
        /// <summary>
        /// ボードを初期化する
        /// </summary>
        public void Initialization()
        {
            int centerPosition = boardSize / 2;

            boardSquaresData[centerPosition - 1, centerPosition - 1] = (SquareState)1;
            boardSquaresData[centerPosition, centerPosition] = (SquareState)1;
            boardSquaresData[centerPosition -1, centerPosition] = (SquareState)2;
            boardSquaresData[centerPosition, centerPosition - 1] = (SquareState)2;
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
            if (playerId < 0 || playerId >= playerNumber) playerId = nextPlayerId;
            if (!CanPutDisc(position, playerId)) return false;

            boardSquaresData[position.x, position.y] = (SquareState)(playerId + 1);

            for (int i = 0; i < 8; i++)
            {
                InversDisc(position, (Direction)(i), playerId);
            }

            NextPlayer(playerId);
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
            return CanPutDisc(new Vector2(x, y));
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
        /// デバッグ用、現在の状況を出力する
        /// </summary>
        public void DebugOutPut()
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
    }
}
