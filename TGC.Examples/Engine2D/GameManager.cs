using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using TGC.Core.Direct3D;
using TGC.Core.UserControls;
using TGC.Examples.Engine2D.Core;

namespace TGC.Examples.Engine2D
{
    internal class GameManager
    {
        //La constante con la cantidad maxima de asteroides simultaneos.
        private const int AsteroidCount = 10;

        public static int ScreenHeight, ScreenWidth;

        private CustomBitmap asteroidBitmap, spaceshipBitmap, misilBitmap;
        private List<Asteroide> asteroids;

        private List<Misil> misiles;
        private Spaceship spaceShip;

        private Drawer2D spriteDrawer;

        public TgcUserVars userVars;

        internal void Init(string exampleDir, TgcUserVars userVars)
        {
            this.userVars = userVars;

            //Creo el sprite drawer
            spriteDrawer = new Drawer2D();

            //Creo la lista de asteroides.
            asteroids = new List<Asteroide>();

            //Creo la lista de misiles.
            misiles = new List<Misil>();

            ScreenWidth = D3DDevice.Instance.Device.Viewport.Width;
            ScreenHeight = D3DDevice.Instance.Device.Viewport.Height;
            //Cargo el bitmap del spritesheet de la nave.
            asteroidBitmap = new CustomBitmap(exampleDir + "Asteroides.png", D3DDevice.Instance.Device);

            spaceshipBitmap = new CustomBitmap(exampleDir + "nave.png", D3DDevice.Instance.Device);

            //Cargo el bitmap del misil que dispara la nave.
            misilBitmap = new CustomBitmap(exampleDir + "particle.png", D3DDevice.Instance.Device);

            //Creo la nave espacial
            spaceShip = new Spaceship();
            spaceShip.Load(spaceshipBitmap);

            //Creo la cantidad de asteroides simultaneos.
            for (var i = 0; i < AsteroidCount; i++)
            {
                var asteroid = new Asteroide();
                asteroid.Load(exampleDir, asteroidBitmap);

                asteroids.Add(asteroid);
            }
        }

        public void fireMissile(Vector2 position, float angle)
        {
            var newMisil = new Misil();

            //Disparo un misil desde la posicion indicada
            newMisil.Position = position;
            newMisil.Angle = angle;
            newMisil.Load(misilBitmap);
            misiles.Add(newMisil);
        }

        //Verifico que misiles se fueron de la pantalla.
        private void checkMisilesRange()
        {
            misiles.RemoveAll(
                delegate (Misil misil)
                {
                    if (misil.Position.X < 0 || misil.Position.Y < 0 ||
                        misil.Position.X > D3DDevice.Instance.Device.Viewport.Width
                        || misil.Position.Y > D3DDevice.Instance.Device.Viewport.Height)
                        return true;
                    return false;
                }
                );
        }

        //Verifico colisiones entre misiles y asteroides.
        private void CheckMisileAsteroidCollisions()
        {
            foreach (var misil in misiles)
            {
                foreach (var asteroide in asteroids)
                {
                    if (
                        Math.Sqrt(Math.Pow(misil.Position.X - asteroide.Position.X, 2.0) +
                                  Math.Pow(misil.Position.Y - asteroide.Position.Y, 2.0)) <= Asteroide.Size)
                        asteroide.GenerateRandomPosition();
                }
            }
        }

        private void CheckSpaceShipAsteroidCollisions()
        {
            //Verifico si la nave colisiona con algun asteroide.
            foreach (var asteroide in asteroids)
            {
                if (
                    Math.Sqrt(Math.Pow(spaceShip.Position.X - asteroide.Position.X, 2.0) +
                              Math.Pow(spaceShip.Position.Y - asteroide.Position.Y, 2.0)) <= 50)
                    spaceShip.RestartPosition();
            }
        }

        internal void Update(float elapsedTime)
        {
            ScreenWidth = D3DDevice.Instance.Device.Viewport.Width;
            ScreenHeight = D3DDevice.Instance.Device.Viewport.Height;

            spaceShip.Update(elapsedTime);
            misiles.ForEach(delegate (Misil m) { m.Update(elapsedTime); });

            asteroids.ForEach(delegate (Asteroide a) { a.Update(elapsedTime); });

            checkMisilesRange();

            CheckMisileAsteroidCollisions();
            CheckSpaceShipAsteroidCollisions();

            userVars.setValue("Misiles", misiles.Count);
        }

        internal void Render(float elapsedTime)
        {
            spriteDrawer.BeginDrawSprite();

            misiles.ForEach(delegate (Misil m) { m.Render(elapsedTime, spriteDrawer); });
            asteroids.ForEach(delegate (Asteroide a) { a.Render(elapsedTime, spriteDrawer); });
            spaceShip.Render(elapsedTime, spriteDrawer);

            spriteDrawer.EndDrawSprite();
        }

        internal void Dispose()
        {
            asteroidBitmap.D3dTexture.Dispose();
            misilBitmap.D3dTexture.Dispose();
            spaceshipBitmap.D3dTexture.Dispose();
        }

        #region Singleton

        private static volatile GameManager instance;
        private static readonly object syncRoot = new object();

        /// <summary>
        ///     Permite acceder a una instancia de la clase GameManager desde cualquier parte del codigo.
        /// </summary>
        public static GameManager Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new GameManager();
                }
                return instance;
            }
        }

        #endregion Singleton
    }
}