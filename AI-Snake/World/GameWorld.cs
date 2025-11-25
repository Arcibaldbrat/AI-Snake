using AISnake.AI;
using AISnake.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AISnake.World
{
    public class GameWorld
    {
        public Snake Snake;
        public Food Food;
        public QLearningAgent Agent;
        public bool UseAI = true;
        public bool Training = false;
        public int Score = 0;

        private GraphicsDevice graphics;
        private int worldWidth, worldHeight;
        private Random rng = new Random();

        public GameWorld(GraphicsDevice graphics, int width, int height)
        {
            this.graphics = graphics;
            worldWidth = width;
            worldHeight = height;

            Snake = new Snake(graphics, new Vector2(width / 2, height / 2), initialSegments: 5, segmentSize: 12f, speed: 150f, color: Color.LimeGreen);
            Food = new Food(graphics, 12, Color.Red);
            Food.SpawnRandom(worldWidth, worldHeight, rng);

            Agent = new QLearningAgent();
        }

        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
           

            // AI rozhodnutí
            if (UseAI)
            {
                int action = Agent.ChooseAction(Snake.Segments[0], Food.Position, Snake.Heading, DangerProbe);
                Snake.SetHeading(Utils.Rotate(Snake.Heading, action - 1));
            }

            // Pohyb hada
            Snake.Move(delta);

            // Update jídla s pohybem
            Food.Update(gameTime);

            // Kontrola kolizí
            if (Snake.CheckOutOfBounds(worldWidth, worldHeight) || Snake.CheckSelfCollision())
            {
                Snake.Kill();
                if (Training && UseAI)
                {
                    // trest za smrt
                    Agent.Learn(Snake.Segments[0], Food.Position, Snake.Heading, DangerProbe, 0, -1, Snake.Segments[0], Snake.Heading);
                }
                Reset();
                return;
            }

            // Kontrola jídla
            if (Vector2.Distance(Snake.Segments[0], Food.Position) < Snake.SegmentSize)
            {
                Snake.Grow();
                Score++;
                Food.SpawnRandom(worldWidth, worldHeight, rng);

                if (Training && UseAI)
                {
                    Agent.Learn(Snake.Segments[0], Food.Position, Snake.Heading, DangerProbe, 0, 1, Snake.Segments[0], Snake.Heading);
                }
            }

            // Q-learning update během tréninku
            if (UseAI && Training)
            {
                Agent.Learn(Snake.Segments[0], Food.Position, Snake.Heading, DangerProbe, 0, 0, Snake.Segments[0], Snake.Heading);
            }

            Snake.Update(gameTime);
            Food.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Snake.Draw(spriteBatch);
            Food.Draw(spriteBatch);
        }

        public void Reset()
        {
            Snake = new Snake(graphics, new Vector2(worldWidth / 2, worldHeight / 2), initialSegments: 5, segmentSize: 12f, speed: 150f, color: Color.LimeGreen);
            Score = 0;
        }

        // Funkce pro zjištění nebezpečí (1 = překážka, 0 = volno)
        public int DangerProbe(Vector2 pos, Direction heading)
        {
            Vector2 dir = Utils.DirectionToVector(heading);
            Vector2 next = pos + dir * Snake.SegmentSize;

            // Kontrola stěn
            if (next.X < 0 || next.Y < 0 || next.X > worldWidth || next.Y > worldHeight) return 1;

            // Kontrola sebe sama
            foreach (var seg in Snake.Segments)
            {
                if (Vector2.Distance(next, seg) < Snake.SegmentSize * 0.5f) return 1;
            }

            return 0;
        }
    }
}
