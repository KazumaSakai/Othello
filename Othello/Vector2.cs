using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Othello
{
    /// <summary>
    /// 位置の構造体
    /// </summary>
    public struct Vector2
    {
        public int x;
        public int y;
    
        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}