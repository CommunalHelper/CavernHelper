using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.CavernHelper {
    [Tracked]
    public class CrystalBombFieldRenderer : Entity {
        private readonly List<CrystalBombField> trackedFields = new();
        private readonly List<Edge> fieldEdges = new();
        private VirtualMap<bool> tiles;
        private Rectangle levelTileBounds;
        private bool dirty;

        public CrystalBombFieldRenderer() {
            Tag = Tags.Global | Tags.TransitionUpdate;
            Depth = 0;
            Add(new CustomBloom(OnRenderBloom));
        }

        public void Track(CrystalBombField block) {
            trackedFields.Add(block);
            if (tiles == null) {
                levelTileBounds = (Scene as Level).TileBounds;
                tiles = new VirtualMap<bool>(levelTileBounds.Width, levelTileBounds.Height, false);
            }

            for (int xTile = (int)block.X / 8; xTile < (block.Right / 8f); xTile++) {
                for (int yTile = (int)block.Y / 8; yTile < (block.Bottom / 8f); yTile++) {
                    tiles[xTile - levelTileBounds.X, yTile - levelTileBounds.Y] = true;
                }
            }

            dirty = true;
        }

        public void Untrack(CrystalBombField block) {
            trackedFields.Remove(block);
            if (trackedFields.Count <= 0) {
                tiles = null;
            } else {
                for (int xTile = (int)block.X / 8; xTile < (block.Right / 8f); xTile++) {
                    for (int yTile = (int)block.Y / 8; yTile < (block.Bottom / 8f); yTile++) {
                        tiles[xTile - levelTileBounds.X, yTile - levelTileBounds.Y] = false;
                    }
                }
            }

            dirty = true;
        }

        public override void Update() {
            if (dirty) {
                RebuildEdges();
            }

            UpdateEdges();
        }

        public void UpdateEdges() {
            Camera camera = (Scene as Level).Camera;
            Rectangle cameraRect = new((int)camera.Left - 4, (int)camera.Top - 4, (int)(camera.Right - camera.Left) + 8, (int)(camera.Bottom - camera.Top) + 8);
            for (int i = 0; i < fieldEdges.Count; i++) {
                if (fieldEdges[i].Visible && Scene.OnInterval(0.25f, i * 0.01f) && !fieldEdges[i].InView(ref cameraRect)) {
                    fieldEdges[i].Visible = false;
                } else
                    if (Scene.OnInterval(0.05f, i * 0.01f) && fieldEdges[i].InView(ref cameraRect)) {
                    fieldEdges[i].Visible = true;
                }

                if (fieldEdges[i].Visible && (Scene.OnInterval(0.05f, i * 0.01f) || fieldEdges[i].Wave == null)) {
                    fieldEdges[i].UpdateWave(Scene.TimeActive * 3f);
                }
            }
        }

        internal static void Load() {
            On.Celeste.LevelLoader.LoadingThread += LevelLoader_LoadingThread;
        }

        internal static void Unload() {
            On.Celeste.LevelLoader.LoadingThread -= LevelLoader_LoadingThread;
        }

        private static void LevelLoader_LoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self) {
            self.Level.Add(new CrystalBombFieldRenderer());
            orig(self);
        }

        private void RebuildEdges() {
            dirty = false;
            fieldEdges.Clear();
            if (trackedFields.Count > 0) {
                Point[] array = new Point[]
                {
                    new Point(0, -1),
                    new Point(0, 1),
                    new Point(-1, 0),
                    new Point(1, 0)
                };

                foreach (CrystalBombField crystalBombDetonator in trackedFields) {
                    for (int xTile = (int)crystalBombDetonator.X / 8; xTile < (crystalBombDetonator.Right / 8f); xTile++) {
                        for (int yTile = (int)crystalBombDetonator.Y / 8; yTile < (crystalBombDetonator.Bottom / 8f); yTile++) {
                            foreach (Point point in array) {
                                Point point2 = new(-point.Y, point.X);
                                if (!Inside(xTile + point.X, yTile + point.Y) && (!Inside(xTile - point2.X, yTile - point2.Y) || Inside(xTile + point.X - point2.X, yTile + point.Y - point2.Y))) {
                                    Point point3 = new(xTile, yTile);
                                    Point point4 = new(xTile + point2.X, yTile + point2.Y);
                                    Vector2 value = new Vector2(4f) + (new Vector2(point.X - point2.X, point.Y - point2.Y) * 4f);
                                    while (Inside(point4.X, point4.Y) && !Inside(point4.X + point.X, point4.Y + point.Y)) {
                                        point4.X += point2.X;
                                        point4.Y += point2.Y;
                                    }

                                    Vector2 a = (new Vector2(point3.X, point3.Y) * 8f) + value - crystalBombDetonator.Position;
                                    Vector2 b = (new Vector2(point4.X, point4.Y) * 8f) + value - crystalBombDetonator.Position;
                                    fieldEdges.Add(new Edge(crystalBombDetonator, a, b));
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool Inside(int tx, int ty) {
            return tiles[tx - levelTileBounds.X, ty - levelTileBounds.Y];
        }

        private void OnRenderBloom() {
            foreach (CrystalBombField crystalBombDetonator in trackedFields) {
                if (crystalBombDetonator.Visible) {
                    Draw.Rect(crystalBombDetonator.X, crystalBombDetonator.Y, crystalBombDetonator.Width, crystalBombDetonator.Height, Color.Purple);
                }
            }

            foreach (Edge edge in fieldEdges) {
                if (edge.Visible) {
                    Vector2 value = edge.Parent.Position + edge.A;
                    Vector2 vector = edge.Parent.Position + edge.B;
                    for (int num = 0; num <= edge.Length; num++) {
                        Vector2 vector2 = value + (edge.Normal * num);
                        Draw.Line(vector2, vector2 + (edge.Perpendicular * edge.Wave[num]), Color.Purple);
                    }
                }
            }
        }

        public override void Render() {
            if (trackedFields.Count <= 0) {
                return;
            }

            Color color = Color.Purple * 0.45f;
            foreach (CrystalBombField crystalBombDetonator in trackedFields) {
                if (crystalBombDetonator.Visible) {
                    Draw.Rect(crystalBombDetonator.Collider, color);
                }
            }

            if (fieldEdges.Count > 0) {
                foreach (Edge edge in fieldEdges) {
                    if (edge.Visible) {
                        Vector2 value2 = edge.Parent.Position + edge.A;
                        for (int num = 0; num <= edge.Length; num++) {
                            Vector2 vector2 = value2 + (edge.Normal * num);
                            Draw.Line(vector2, vector2 + (edge.Perpendicular * edge.Wave[num]), color);
                        }
                    }
                }
            }
        }

        private class Edge {
            public CrystalBombField Parent;
            public bool Visible = true;
            public Vector2 A;
            public Vector2 B;
            public Vector2 Min;
            public Vector2 Max;
            public Vector2 Normal;
            public Vector2 Perpendicular;
            public float[] Wave;
            public float Length;

            public Edge(CrystalBombField parent, Vector2 a, Vector2 b) {
                Parent = parent;
                A = a;
                B = b;
                Min = new Vector2(Math.Min(A.X, B.X), Math.Min(A.Y, B.Y));
                Max = new Vector2(Math.Max(A.X, B.X), Math.Max(A.Y, B.Y));
                Normal = (B - A).SafeNormalize();
                Perpendicular = -Normal.Perpendicular();
                Length = (A - B).Length();
            }

            public void UpdateWave(float time) {
                if (Wave == null || Wave.Length <= Length) {
                    Wave = new float[(int)Length + 2];
                }

                for (int num = 0; num <= Length; num++) {
                    Wave[num] = GetWaveAt(time, num, Length);
                }
            }

            private float GetWaveAt(float offset, float along, float length) {
                if (along <= 1f || along >= length - 1f) {
                    return 0f;
                }

                if (Parent.Solidify >= 1f) {
                    return 0f;
                }

                float num = offset + (along * 0.25f);
                float num2 = (float)((Math.Sin((double)num) * 2.0) + Math.Sin((double)(num * 0.25f)));
                return (1f + (num2 * Ease.SineInOut(Calc.YoYo(along / length)))) * (1f - Parent.Solidify);
            }

            public bool InView(ref Rectangle view) {
                return view.Left < Parent.X + Max.X && view.Right > Parent.X + Min.X && view.Top < Parent.Y + Max.Y && view.Bottom > Parent.Y + Min.Y;
            }
        }
    }
}
