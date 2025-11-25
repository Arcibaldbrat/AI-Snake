using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace AISnake.Entities
{
    public enum Direction { Up, Down, Left, Right }

    public class Snake : IEntity
    {
        public List<Vector2> Segments = new List<Vector2>();
        public Direction Heading = Direction.Right;

        public float Speed;
        public float SegmentSize;
        public bool Dead = false;

        private GraphicsDevice graphics;
        private Texture2D tex;

        // Plynulé následování - ukládáme trajektorii hlavy
        private List<Vector2> pathHistory = new List<Vector2>();
        private float followSpacing = 8f; // menší = plynulejší, větší = delší tělo

        public Snake(GraphicsDevice graphics, Vector2 startPos,
                     int initialSegments, float segmentSize, float speed, Color color)
        {
            this.graphics = graphics;
            Speed = speed;
            SegmentSize = segmentSize;

            tex = CreateRect(graphics, (int)segmentSize, (int)segmentSize, color);

            Segments.Clear();
            for (int i = 0; i < initialSegments; i++)
                Segments.Add(startPos + new Vector2(-i * (segmentSize * 0.7f), 0));

            // Inicializace historie
            pathHistory.Clear();
            pathHistory.Add(startPos);
        }

        public void SetHeading(Direction dir)
        {
            // zabrání otočení o 180°
            if ((Heading == Direction.Up && dir == Direction.Down) ||
                (Heading == Direction.Down && dir == Direction.Up) ||
                (Heading == Direction.Left && dir == Direction.Right) ||
                (Heading == Direction.Right && dir == Direction.Left))
                return;

            Heading = dir;
        }

        public void Move(float delta)
        {
            if (Dead) return;

            // pohyb hlavy
            Vector2 head = Segments[0];
            Vector2 velocity = DirectionToVector(Heading) * Speed * delta;

            head += velocity;
            Segments[0] = head;

            // uložení trajektorie
            pathHistory.Insert(0, head);

            // následování segmentů
            for (int i = 1; i < Segments.Count; i++)
            {
                float targetDist = i * followSpacing;

                int index = (int)(targetDist);

                if (index < pathHistory.Count)
                    Segments[i] = pathHistory[index];
            }

            // čistíme historii aby nerostla donekonečna
            if (pathHistory.Count > 5000)
                pathHistory.RemoveRange(2000, pathHistory.Count - 2000);
        }

        public void Grow()
        {
            // nový segment na poslední známé pozici
            Segments.Add(Segments[Segments.Count - 1]);
        }

        public bool CheckOutOfBounds(int width, int height)
        {
            var h = Segments[0];
            return (h.X < 0 || h.Y < 0 || h.X > width || h.Y > height);
        }

        public bool CheckSelfCollision()
        {
            for (int i = 2; i < Segments.Count; i++)
            {
                if (Vector2.Distance(Segments[0], Segments[i]) < SegmentSize * 0.5f)
                    return true;
            }
            return false;
        }

        public void Kill()
        {
            Dead = true;
        }

        public void Update(GameTime gameTime) { }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var seg in Segments)
                spriteBatch.Draw(tex, new Rectangle((int)seg.X, (int)seg.Y,
                    (int)SegmentSize, (int)SegmentSize), Color.White);
        }

        private Texture2D CreateRect(GraphicsDevice g, int w, int h, Color c)
        {
            Texture2D t = new Texture2D(g, w, h);
            Color[] data = new Color[w * h];
            for (int i = 0; i < data.Length; i++) data[i] = c;
            t.SetData(data);
            return t;
        }

        private Vector2 DirectionToVector(Direction d)
        {
            switch (d)
            {
                case Direction.Up: return new Vector2(0, -1);
                case Direction.Down: return new Vector2(0, 1);
                case Direction.Left: return new Vector2(-1, 0);
                default: return new Vector2(1, 0);
            }
        }
    }
}
