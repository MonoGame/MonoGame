using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Asteroid_Belt_Assault
{
    class EnemyManager
    {
        private Texture2D texture;
        private Rectangle initialFrame;
        private int frameCount;

        public List<Enemy> Enemies = new List<Enemy>();

        public ShotManager EnemyShotManager;
        private PlayerManager playerManager;

        public int MinShipsPerWave = 5;
        public int MaxShipsPerWave = 8;
        private float nextWaveTimer = 0.0f;
        private float nextWaveMinTimer = 8.0f;
        private float shipSpawnTimer = 0.0f;
        private float shipSpawnWaitTime = 0.5f;

        private float shipShotChance = 0.2f;

        private List<List<Vector2>> pathWaypoints =
            new List<List<Vector2>>();

        private Dictionary<int, int> waveSpawns = new Dictionary<int, int>();

        public bool Active = false;

        private Random rand = new Random();

        private void setUpWaypoints()
        {
            List<Vector2> path0 = new List<Vector2>();
            path0.Add(new Vector2(850, 300));
            path0.Add(new Vector2(-100, 300));
            pathWaypoints.Add(path0);
            waveSpawns[0] = 0;

            List<Vector2> path1 = new List<Vector2>();
            path1.Add(new Vector2(-50, 225));
            path1.Add(new Vector2(850, 225));
            pathWaypoints.Add(path1);
            waveSpawns[1] = 0;

            List<Vector2> path2 = new List<Vector2>();
            path2.Add(new Vector2(-100, 50));
            path2.Add(new Vector2(150, 50));
            path2.Add(new Vector2(200, 75));
            path2.Add(new Vector2(200, 125));
            path2.Add(new Vector2(150, 150));
            path2.Add(new Vector2(150, 175));
            path2.Add(new Vector2(200, 200));
            path2.Add(new Vector2(600, 200));
            path2.Add(new Vector2(850, 600));
            pathWaypoints.Add(path2);
            waveSpawns[2] = 0;

            List<Vector2> path3 = new List<Vector2>();
            path3.Add(new Vector2(600, -100));
            path3.Add(new Vector2(600, 250));
            path3.Add(new Vector2(580, 275));
            path3.Add(new Vector2(500, 250));
            path3.Add(new Vector2(500, 200));
            path3.Add(new Vector2(450, 175));
            path3.Add(new Vector2(400, 150));
            path3.Add(new Vector2(-100, 150));
            pathWaypoints.Add(path3);
            waveSpawns[3] = 0;
        }

        public EnemyManager(
            Texture2D texture,
            Rectangle initialFrame,
            int frameCount,
            PlayerManager playerManager,
            Rectangle screenBounds)
        {
            this.texture = texture;
            this.initialFrame = initialFrame;
            this.frameCount = frameCount;
            this.playerManager = playerManager;

            EnemyShotManager = new ShotManager(
                texture,
                new Rectangle(0, 300, 5, 5),
                    4,
                    2,
                    150f,
                    screenBounds);

            setUpWaypoints();
        }

        public void SpawnEnemy(int path)
        {
            Enemy thisEnemy = new Enemy(
                texture,
                pathWaypoints[path][0],
                initialFrame,
                frameCount);
            for (int x = 0; x < pathWaypoints[path].Count(); x++)
            {
                thisEnemy.AddWaypoint(pathWaypoints[path][x]);
            }
            Enemies.Add(thisEnemy);
        }

        public void SpawnWave(int waveType)
        {
            waveSpawns[waveType] +=
                rand.Next(MinShipsPerWave, MaxShipsPerWave + 1);
        }

        private void updateWaveSpawns(GameTime gameTime)
        {
            shipSpawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (shipSpawnTimer > shipSpawnWaitTime)
            {
                for (int x = waveSpawns.Count - 1; x >= 0; x--)
                {
                    if (waveSpawns[x] > 0)
                    {
                        waveSpawns[x]--;
                        SpawnEnemy(x);
                    }
                }
                shipSpawnTimer = 0f;
            }

            nextWaveTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (nextWaveTimer > nextWaveMinTimer)
            {
                SpawnWave(rand.Next(0, pathWaypoints.Count));
                nextWaveTimer = 0f;
            }
        }

        public void Update(GameTime gameTime)
        {
            EnemyShotManager.Update(gameTime);

            for (int x = Enemies.Count - 1; x >= 0; x--)
            {
                Enemies[x].Update(gameTime);
                if (Enemies[x].IsActive() == false)
                {
                    Enemies.RemoveAt(x);
                }
                else
                {
                    if ((float)rand.Next(0, 1000) / 10 <= shipShotChance)
                    {
                        Vector2 fireLoc = Enemies[x].EnemySprite.Location;
                        fireLoc += Enemies[x].gunOffset;

                        Vector2 shotDirection =
                            playerManager.playerSprite.Center -
                            fireLoc;

                        shotDirection.Normalize();

                        EnemyShotManager.FireShot(
                            fireLoc,
                            shotDirection,
                            false);
                    }
                }
            }

            if (Active)
            {
                updateWaveSpawns(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            EnemyShotManager.Draw(spriteBatch);

            foreach (Enemy enemy in Enemies)
            {
                enemy.Draw(spriteBatch);
            }
        }

    }
}
