using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Othello
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            OthelloBoard othelloBoard = new OthelloBoard();
            othelloBoard.DebugOutPut();

            while (true)
            {
                string[] xy = Console.ReadLine().Split(' ');

                int x = int.Parse(xy[0]) - 1;
                int y = int.Parse(xy[1]) - 1;

                othelloBoard.PutDisc(new Vector2(x, y));
                othelloBoard.DebugOutPut();
            }

            /*
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            */
        }
    }
}
