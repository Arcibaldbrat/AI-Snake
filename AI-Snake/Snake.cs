using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SnakeGame
{
    public class Snake
    {
        public List<Point> Body { get; private set; }
        public Point Direction { get; set; }
        public Color CurrentColor { get; set; }
        public bool HasPowerUp { get; set; }
        public float PowerUpTimer { get; set; }

        private const int GRID_SIZE = 20;

        public Snake(int startX, int startY)
        {
            Body = new List<Point>();
            Body.Add(new Point(startX, startY));
            Direction = new Point(1, 0);
            CurrentColor = Color.Green;
            HasPowerUp = false;
            PowerUpTimer = 0;
        }

        public void Move()
        {
            Point newHead = new Point(Body[0].X + Direction.X, Body[0].Y + Direction.Y);
            Body.Insert(0, newHead);
        }

        public void RemoveTail()
        {
            if (Body.Count > 0)
                Body.RemoveAt(Body.Count - 1);
        }

        public bool CheckCollision()
        {
            Point head = Body[0];

            // Kolize se zdí
            if (head.X < 0 || head.X >= GRID_SIZE || head.Y < 0 || head.Y >= GRID_SIZE)
                return true;

            // Kolize se sebou
            for (int i = 1; i < Body.Count; i++)
            {
                if (Body[i] == head)
                    return true;
            }

            return false;
        }

        public bool CheckCollisionWith(Point position)
        {
            return Body[0] == position;
        }

        public bool BodyContains(Point position)
        {
            return Body.Contains(position);
        }

        public void UpdatePowerUp(float deltaTime)
        {
            if (HasPowerUp)
            {
                PowerUpTimer -= deltaTime;
                if (PowerUpTimer <= 0)
                {
                    HasPowerUp = false;
                    CurrentColor = Color.Green;
                }
            }
        }

        public void ChangeDirection(Point newDirection)
        {
            // Prevence otočení o 180 stupňů
            if ((Direction.X != 0 && newDirection.X != 0) ||
                (Direction.Y != 0 && newDirection.Y != 0))
                return;

            Direction = newDirection;
        }
    }
}