using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SnakeGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private const int GRID_SIZE = 20;
        private const int CELL_SIZE = 30;
        private const float BASE_SPEED = 0.15f;

        private Snake snake;
        private List<ICollectable> collectables;
        private List<Point> obstacles;
        private Random random;

        private float moveTimer;
        private int score;
        private bool gameOver;
        private float currentSpeed;
        private int foodCount;

        private Texture2D pixel;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = GRID_SIZE * CELL_SIZE;
            _graphics.PreferredBackBufferHeight = GRID_SIZE * CELL_SIZE + 80;
        }

        protected override void Initialize()
        {
            random = new Random();
            ResetGame();
            base.Initialize();
        }

        private void ResetGame()
        {
            snake = new Snake(GRID_SIZE / 2, GRID_SIZE / 2);
            collectables = new List<ICollectable>();
            obstacles = new List<Point>();

            score = 0;
            gameOver = false;
            currentSpeed = BASE_SPEED;
            foodCount = 0;
            moveTimer = 0;

            SpawnFood(Food.FoodType.Normal);
            SpawnObstacles(5);
        }

        private void SpawnFood(Food.FoodType type)
        {
            Food food = new Food(type);
            food.Spawn(random, snake);
            collectables.Add(food);
        }

        private void SpawnObstacles(int count)
        {
            obstacles.Clear();
            for (int i = 0; i < count; i++)
            {
                Point obstacle;
                int attempts = 0;
                do
                {
                    obstacle = new Point(random.Next(GRID_SIZE), random.Next(GRID_SIZE));
                    attempts++;
                }
                while ((snake.BodyContains(obstacle) || obstacles.Contains(obstacle) ||
                       IsNearSnakeHead(obstacle, 3)) && attempts < 50);

                if (attempts < 50)
                    obstacles.Add(obstacle);
            }
        }

        private bool IsNearSnakeHead(Point pos, int distance)
        {
            Point head = snake.Body[0];
            return Math.Abs(head.X - pos.X) < distance && Math.Abs(head.Y - pos.Y) < distance;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // UI text v titulku okna
            Window.Title = $"Had - Skore: {score} | Delka: {snake.Body.Count} | Jidel: {foodCount}";

            if (snake.HasPowerUp)
            {
                string powerText = snake.CurrentColor == Color.Cyan ? "RYCHLOST" : "2x BODY";
                Window.Title += $" | {powerText}: {snake.PowerUpTimer:F1}s";
            }

            if (gameOver)
            {
                Window.Title += " | KONEC HRY! Stiskni MEZERNK";
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                    ResetGame();
                return;
            }

            // Ovládání
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Up))
                snake.ChangeDirection(new Point(0, -1));
            else if (state.IsKeyDown(Keys.Down))
                snake.ChangeDirection(new Point(0, 1));
            else if (state.IsKeyDown(Keys.Left))
                snake.ChangeDirection(new Point(-1, 0));
            else if (state.IsKeyDown(Keys.Right))
                snake.ChangeDirection(new Point(1, 0));

            // Update power-upů
            snake.UpdatePowerUp((float)gameTime.ElapsedGameTime.TotalSeconds);

            // Rychlost podle power-upu
            currentSpeed = snake.HasPowerUp && snake.CurrentColor == Color.Cyan
                ? BASE_SPEED * 0.6f
                : Math.Max(0.06f, BASE_SPEED - (foodCount * 0.004f));

            // Pohyb hada
            moveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (moveTimer >= currentSpeed)
            {
                moveTimer = 0;

                snake.Move();

                // Kontrola kolizí
                if (snake.CheckCollision())
                {
                    gameOver = true;
                    return;
                }

                // Kontrola kolize s překážkami
                foreach (Point obstacle in obstacles)
                {
                    if (snake.CheckCollisionWith(obstacle))
                    {
                        gameOver = true;
                        return;
                    }
                }

                // Kontrola sběru collectables
                bool collected = false;
                foreach (ICollectable item in collectables)
                {
                    if (item.IsActive && snake.CheckCollisionWith(item.Position))
                    {
                        item.OnCollect(snake, ref score);
                        collected = true;
                        foodCount++;

                        // Každých 5 jídel přidej překážky
                        if (foodCount % 5 == 0)
                            SpawnObstacles(5 + foodCount / 5);

                        break;
                    }
                }

                if (collected)
                {
                    collectables.RemoveAll(c => !c.IsActive);

                    // Spawn nového jídla
                    Food.FoodType nextType = Food.FoodType.Normal;
                    if (foodCount % 7 == 0)
                        nextType = Food.FoodType.GrowthBoost;
                    else if (foodCount % 5 == 0)
                        nextType = (Food.FoodType)random.Next(1, 3);

                    SpawnFood(nextType);
                }
                else
                {
                    snake.RemoveTail();
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();

            // Mřížka
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    Color cellColor = (x + y) % 2 == 0 ? new Color(15, 15, 15) : new Color(25, 25, 25);
                    _spriteBatch.Draw(pixel,
                        new Rectangle(x * CELL_SIZE, y * CELL_SIZE, CELL_SIZE, CELL_SIZE),
                        cellColor);
                }
            }

            // Překážky
            foreach (Point obstacle in obstacles)
            {
                _spriteBatch.Draw(pixel,
                    new Rectangle(obstacle.X * CELL_SIZE + 2, obstacle.Y * CELL_SIZE + 2,
                    CELL_SIZE - 4, CELL_SIZE - 4),
                    Color.DarkGray);
            }

            // Collectables
            foreach (ICollectable item in collectables)
            {
                if (item.IsActive)
                {
                    int size = item is Food food && food.Type != Food.FoodType.Normal
                        ? CELL_SIZE - 6 : CELL_SIZE - 8;
                    int offset = item is Food food2 && food2.Type != Food.FoodType.Normal
                        ? 3 : 4;

                    _spriteBatch.Draw(pixel,
                        new Rectangle(item.Position.X * CELL_SIZE + offset,
                                    item.Position.Y * CELL_SIZE + offset,
                                    size, size),
                        item.GetColor());
                }
            }

            // Had
            for (int i = 0; i < snake.Body.Count; i++)
            {
                Color color = i == 0 ? Color.Lerp(snake.CurrentColor, Color.White, 0.3f) : snake.CurrentColor;
                int size = i == 0 ? CELL_SIZE - 2 : CELL_SIZE - 4;
                int offset = i == 0 ? 1 : 2;

                _spriteBatch.Draw(pixel,
                    new Rectangle(snake.Body[i].X * CELL_SIZE + offset,
                                snake.Body[i].Y * CELL_SIZE + offset,
                                size, size),
                    color);
            }

            // UI text je v titulku okna (viz Update metoda)

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}