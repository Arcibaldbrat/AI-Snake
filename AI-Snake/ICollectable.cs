using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace SnakeGame
{
    public interface ICollectable
    {
        Point Position { get; set; }
        Color GetColor();
        void OnCollect(Snake snake, ref int score);
        bool IsActive { get; set; }
    }
}