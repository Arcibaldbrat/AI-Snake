using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AISnake.Entities;

namespace AISnake.AI
{
    public class QLearningAgent
    {
        private Dictionary<string, float[]> qTable = new Dictionary<string, float[]>();
        private Random rng = new Random();
        public float Alpha = 0.2f;
        public float Gamma = 0.9f;
        public float Epsilon = 0.15f;

        private string StateKey(int dxSign, int dySign, int front, int left, int right)
        {
            return $"{dxSign}:{dySign}:{front}:{left}:{right}";
        }

        private int SignInt(float v)
        {
            if (v > 8) return 1;
            if (v < -8) return -1;
            return 0;
        }

        public int ChooseAction(Vector2 head, Vector2 food, Direction heading, Func<Vector2, Direction, int> dangerProbe)
        {
            float dx = food.X - head.X;
            float dy = food.Y - head.Y;
            int dxs = SignInt(dx);
            int dys = SignInt(dy);
            int front = dangerProbe(head, heading);
            int left = dangerProbe(head, Utils.Rotate(heading, -1));
            int right = dangerProbe(head, Utils.Rotate(heading, 1));

            string key = StateKey(dxs, dys, front, left, right);
            EnsureState(key);

            if (rng.NextDouble() < Epsilon) return rng.Next(0, 3);

            float[] q = qTable[key];
            int best = 0;
            float bestv = q[0];
            for (int i = 1; i < q.Length; i++)
            {
                if (q[i] > bestv) { bestv = q[i]; best = i; }
            }
            return best;
        }

        public void Learn(Vector2 head, Vector2 food, Direction heading, Func<Vector2, Direction, int> dangerProbe, int actionIndex, float reward, Vector2 newHead, Direction newHeading)
        {
            int dx = SignInt(food.X - head.X);
            int dy = SignInt(food.Y - head.Y);
            int f = dangerProbe(head, heading);
            int l = dangerProbe(head, Utils.Rotate(heading, -1));
            int r = dangerProbe(head, Utils.Rotate(heading, 1));
            string key = StateKey(dx, dy, f, l, r);
            EnsureState(key);

            int ndx = SignInt(food.X - newHead.X);
            int ndy = SignInt(food.Y - newHead.Y);
            int nf = dangerProbe(newHead, newHeading);
            int nl = dangerProbe(newHead, Utils.Rotate(newHeading, -1));
            int nr = dangerProbe(newHead, Utils.Rotate(newHeading, 1));
            string nextKey = StateKey(ndx, ndy, nf, nl, nr);
            EnsureState(nextKey);

            float q = qTable[key][actionIndex];
            float maxNext = Max(qTable[nextKey]);
            float updated = q + Alpha * (reward + Gamma * maxNext - q);
            qTable[key][actionIndex] = updated;
        }

        private float Max(float[] arr)
        {
            float m = arr[0];
            for (int i = 1; i < arr.Length; i++) if (arr[i] > m) m = arr[i];
            return m;
        }

        private void EnsureState(string key)
        {
            if (!qTable.ContainsKey(key))
                qTable[key] = new float[3] { 0f, 0f, 0f };
        }

        public void Save(string path)
        {
            var json = JsonSerializer.Serialize(qTable);
            File.WriteAllText(path, json);
        }

        public void Load(string path)
        {
            if (!File.Exists(path)) return;
            var json = File.ReadAllText(path);
            qTable = JsonSerializer.Deserialize<Dictionary<string, float[]>>(json);
        }
    }
}
