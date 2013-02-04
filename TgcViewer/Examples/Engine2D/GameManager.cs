using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer;
using Microsoft.DirectX;
using System.Drawing;

namespace Examples.Engine2D
{
    class GameManager
    {

        #region Singleton
        private static volatile GameManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Permite acceder a una instancia de la clase GameManager desde cualquier parte del codigo.
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
        #endregion

        Drawer spriteDrawer;
        Spaceship spaceShip;
        List<Asteroide> asteroids;
        List<Misil> misiles;


        string exampleDir;

        //La constante con la cantidad maxima de asteroides simultaneos.
        const int AsteroidCount = 10;

        public static int ScreenHeight, ScreenWidth;

        Bitmap asteroidBitmap, spaceshipBitmap, misilBitmap;

        internal void Init(string exampleDir)
        {
            this.exampleDir = exampleDir;

            //Creo el sprite drawer
            spriteDrawer = new Drawer();

            //Creo la lista de asteroides.
            asteroids = new List<Asteroide>();

            //Creo la lista de misiles.
            misiles = new List<Misil>();

                        
            ScreenWidth = GuiController.Instance.D3dDevice.Viewport.Width;
            ScreenHeight = GuiController.Instance.D3dDevice.Viewport.Height;
            //Cargo el bitmap del spritesheet de la nave.
            asteroidBitmap = new Bitmap(exampleDir + "Engine2D\\Media\\" + "Asteroides.png", GuiController.Instance.D3dDevice);

            spaceshipBitmap = new Bitmap(exampleDir + "Engine2D\\Media\\" + "nave.png", GuiController.Instance.D3dDevice);

            //Cargo el bitmap del misil que dispara la nave.
            misilBitmap = new Bitmap(exampleDir + "Engine2D\\Media\\" + "particle.png", GuiController.Instance.D3dDevice);

            //Creo la nave espacial
            spaceShip = new Spaceship();
            spaceShip.Load(exampleDir, spaceshipBitmap);

            //Creo la cantidad de asteroides simultaneos.
            for (int i = 0; i < AsteroidCount; i++)
            {
                Asteroide asteroid = new Asteroide();
                asteroid.Load(exampleDir, asteroidBitmap);

                asteroids.Add(asteroid);
            }

        }

        public void fireMissile(Vector2 position, float angle)
        {
             Misil newMisil = new Misil();
            
             //Disparo un misil desde la posicion indicada
             newMisil.Position = position;
             newMisil.Angle = angle;
             newMisil.Load(exampleDir, misilBitmap);
             misiles.Add(newMisil);
        }

        //Verifico que misiles se fueron de la pantalla.
        private void checkMisilesRange()
        {

            misiles.RemoveAll(
                delegate(Misil misil)
                {
                    if (misil.Position.X < 0 || misil.Position.Y < 0 ||
                        misil.Position.X > GuiController.Instance.D3dDevice.Viewport.Width
                        || misil.Position.Y > GuiController.Instance.D3dDevice.Viewport.Height)
                        return true;
                    else
                        return false;
                }

             );

        }

        //Verifico colisiones entre misiles y asteroides.
        private void CheckMisileAsteroidCollisions()
        {
            foreach (Misil misil in misiles)
            {

                foreach (Asteroide asteroide in asteroids)
                {
                    if (Math.Sqrt(Math.Pow(misil.Position.X - asteroide.Position.X, 2.0) + Math.Pow(misil.Position.Y - asteroide.Position.Y, 2.0)) <= Asteroide.Size)
                        asteroide.GenerateRandomPosition();
                }
            }
        }

        private void CheckSpaceShipAsteroidCollisions()
        {
            //Verifico si la nave colisiona con algun asteroide.
            foreach (Asteroide asteroide in asteroids)
            {
                if (Math.Sqrt(Math.Pow(spaceShip.Position.X - asteroide.Position.X, 2.0) + Math.Pow(spaceShip.Position.Y - asteroide.Position.Y, 2.0)) <= 50)
                    spaceShip.RestartPosition();
            }

        }

        internal void Update(float elapsedTime)
        {

            ScreenWidth = GuiController.Instance.D3dDevice.Viewport.Width;
            ScreenHeight = GuiController.Instance.D3dDevice.Viewport.Height;

            spaceShip.Update(elapsedTime);
            misiles.ForEach(delegate(Misil m) { m.Update(elapsedTime); });

            asteroids.ForEach(delegate(Asteroide a) { a.Update(elapsedTime); });

            checkMisilesRange();

            CheckMisileAsteroidCollisions();
            CheckSpaceShipAsteroidCollisions();

            GuiController.Instance.UserVars.setValue("Misiles", misiles.Count);
            
        }

        internal void Render(float elapsedTime)
        {
            spriteDrawer.BeginDrawSprite();

            misiles.ForEach(delegate(Misil m) { m.Render(elapsedTime,spriteDrawer); });
            asteroids.ForEach(delegate(Asteroide a) { a.Render(elapsedTime, spriteDrawer); });
            spaceShip.Render(elapsedTime, spriteDrawer);

            spriteDrawer.EndDrawSprite();
        }


    }
}
