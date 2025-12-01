using Microsoft.Xna.Framework;
using System;

namespace SnakeGame
{
    public class Food : ICollectable
    {
        public Point Position { get; set; }
        public bool IsActive { get; set; }
        public FoodType Type { get; set; }

        private const int GRID_SIZE = 20;

        public enum FoodType
        {
            Normal,
            SpeedBoost,
            DoublePoints,
            GrowthBoost
        }

        public Food(FoodType type = FoodType.Normal)
        {
            Type = type;
            IsActive = true;
        }

        public void Spawn(Random random, Snake snake)
        {
            do
            {
                Position = new Point(random.Next(GRID_SIZE), random.Next(GRID_SIZE));
            }
            while (snake.BodyContains(Position));

            IsActive = true;
        }

        public Color GetColor()
        {
            return Type switch
            {
                FoodType.Normal => Color.Red,
                FoodType.SpeedBoost => Color.Cyan,
                FoodType.DoublePoints => Color.Yellow,
                FoodType.GrowthBoost => Color.Magenta,
                _ => Color.White
            };
        }

        public void OnCollect(Snake snake, ref int score)
        {
            switch (Type)
            {
                case FoodType.Normal:
                    score += 10;
                    break;

                case FoodType.SpeedBoost:
                    score += 15;
                    snake.HasPowerUp = true;
                    snake.PowerUpTimer = 5f;
                    snake.CurrentColor = Color.Cyan;
                    break;

                case FoodType.DoublePoints:
                    score += 30;
                    snake.HasPowerUp = true;
                    snake.PowerUpTimer = 5f;
                    snake.CurrentColor = Color.Yellow;
                    break;

                case FoodType.GrowthBoost:
                    score += 20;
                    // Had vyroste o 3 segmenty navíc
                    for (int i = 0; i < 2; i++)
                    {
                        snake.Body.Add(snake.Body[snake.Body.Count - 1]);
                    }
                    break;
            }

            IsActive = false;
        }
    }
}