global using System.Drawing;
global using System.Drawing.Drawing2D;
using BeeEngine.Collections;

namespace BeeEngine.Drawing
{
    internal class RenderingQueue: IDisposable
    {
        private List<Sprite2D> ToAdd = new List<Sprite2D>();
        private List<Sprite2D> ToRemove = new List<Sprite2D>();
        private WeakCollection<Sprite2D>[] sprites = new WeakCollection<Sprite2D>[5]
        {
            new WeakCollection<Sprite2D>(),
            new WeakCollection<Sprite2D>(),
            new WeakCollection<Sprite2D>(),
            new WeakCollection<Sprite2D>(),
            new WeakCollection<Sprite2D>()
        };

        private readonly GameEngine _instance;
        public RenderingQueue(GameEngine instance)
        {
            _instance = instance;
        }
        internal void Render(FastGraphics g)
        {
            //AllSprites.SortSprites();
            g.IsFrameGraphics = true;
            g.Clear(_instance.BackgroundColor);
            Camera.Move(g);
            /*foreach(var sprite in AllSprites)
            {
                lock(sprite)
                {
                    sprite.PaintIt(g);
                }
            }*/
            foreach (var spriteList in sprites)
            {
                for (int i = 0; i < spriteList.Count; i++)
                {
                    lock (spriteList[i])
                    {
                        spriteList[i].PaintIt(g);
                    }
                }
            }

            Add();
            Remove();
            //Log.Info($"Sprite0: {sprites[0].Count}, Sprite1: {sprites[1].Count}, Sprite2: {sprites[2].Count}, Sprite3: {sprites[3].Count}, Sprite4: {sprites[4].Count}");
        }

        public void Dispose()
        {
            foreach (var spriteList in sprites)
            {
                for (int i = 0; i < spriteList.Count; i++)
                {
                    spriteList[i]?.Dispose();
                }
            }
        }

        internal void AddSprite(Sprite2D sprite)
        {
            ToAdd.Add(sprite);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add()
        {
            int t = ToAdd.Count;
            for (int i = 0; i < t; i++)
            {
                //AllSprites.Add(ToAdd[i]);
                sprites[4 - ToAdd[i].Priority].Add(ToAdd[i]);
            }
            ToAdd.RemoveRange(0, t);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Remove()
        {
            int t = ToRemove.Count;
            for (int i = 0; i < t; i++)
            {
                //AllSprites.Remove(ToRemove[i]);
                sprites[4 - ToRemove[i].Priority].Remove(ToRemove[i]);
            }
            ToRemove.RemoveRange(0, t);
        }
        internal void RemoveSprite(Sprite2D sprite)
        {
            ToRemove.Add(sprite);
        }
    }
}
