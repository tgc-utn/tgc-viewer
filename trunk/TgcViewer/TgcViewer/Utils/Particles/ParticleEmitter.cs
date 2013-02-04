using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils;
using System.Collections;
using TgcViewer.Utils.TgcSceneLoader;
using System.Collections.Generic;

namespace TgcViewer.Utils.Particles
{
    /// <summary>
    /// Emisor de particulas para generar efectos de particulas
    /// </summary>
    public class ParticleEmitter
    {
        protected int particlesCount;
        protected Particle[] particles;
        protected ColaDeParticulas particlesAlive;
        protected Stack<Particle> particlesDead;
        protected float tiempoAcumulado = 0;
        protected Random random;
        protected VertexDeclaration vertexDeclaration;
        protected Particle.ParticleVertex[] particleVertexArray;

        protected bool enabled;
        /// <summary>
        /// Habilita o deshabilita el emisor
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        protected bool playing;
        /// <summary>
        /// Iniciar o parar la generacion de nuevas particulas
        /// </summary>
        public bool Playing
        {
            get { return playing; }
            set { playing = value; }
        }

        protected Vector3 position;
        /// <summary>
        /// Posicion del emisor de particulas en la escena.
        /// Todas las particulas se generan en esta posicion.
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        protected float creationFrecuency;
        /// <summary>
        /// Tiempo de frecuencia de creacion de particulas.
        /// Cuanto mas chico mas rapido.
        /// </summary>
        public float CreationFrecuency
        {
            get { return this.creationFrecuency; }
            set { this.creationFrecuency = value; }
        }

        protected float particleTimeToLive;
        /// <summary>
        /// Tiempo de vida de las particulas.
        /// </summary>
        public float ParticleTimeToLive
        {
            get { return this.particleTimeToLive; }
            set { this.particleTimeToLive = value; }
        }

        protected float minSizeParticle;
        /// <summary>
        /// Minimo tamaño que puede tener una particula.
        /// </summary>
        public float MinSizeParticle
        {
            get { return this.minSizeParticle; }
            set { this.minSizeParticle = value; }
        }

        protected float maxSizeParticle;
        /// <summary>
        /// Maximo tamaño que puede tener una particula.
        /// </summary>
        public float MaxSizeParticle
        {
            get { return this.maxSizeParticle; }
            set { this.maxSizeParticle = value; }
        }

        protected int dispersion;
        /// <summary>
        /// Valor que representa el grado de dispersion de las particulas.
        /// </summary>
        public int Dispersion
        {
            get { return this.dispersion; }
            set { this.dispersion = value;}
        }

        protected Vector3 speed;
        /// <summary>
        /// Vector que se le multiplica a la velocidad base (Por defecto es (1,1,1)).
        /// Si Y es negativo las particulas descienden.
        /// </summary>
        public Vector3 Speed
        {
            get { return this.speed; }
            set { this.speed = value; }
        }

        protected TgcTexture texture;
        /// <summary>
        /// Textura utilizada
        /// </summary>
        public TgcTexture Texture
        {
            get { return texture; }
        }


        /// <summary>
        /// Crear un emisor de particulas
        /// </summary>
        /// <param name="texturePath">textura a utilizar</param>
        /// <param name="particlesCount">cantidad maxima de particlas a generar</param>
        public ParticleEmitter(string texturePath, int particlesCount)
        {
            Device device = GuiController.Instance.D3dDevice;

            //valores default
            enabled = true;
            playing = true;
            random = new Random(0);
            creationFrecuency = 1.0f;
            particleTimeToLive = 5.0f;
            minSizeParticle = 0.25f;
            maxSizeParticle = 0.5f;
            dispersion = 100;
            speed = new Vector3(1, 1, 1);
            particleVertexArray = new Particle.ParticleVertex[1];
            vertexDeclaration = new VertexDeclaration(device, Particle.ParticleVertexElements);
            position = new Vector3(0, 0, 0);

            this.particlesCount = particlesCount;
            this.particles = new Particle[particlesCount];

            this.particlesAlive = new ColaDeParticulas(particlesCount);
            this.particlesDead = new Stack<Particle>(particlesCount);

            //Creo todas las particulas. Inicialmente estan todas muertas.
            for (int i = 0; i < particlesCount; i++)
            {
                this.particles[i] = new Particle();
                this.particlesDead.Push(this.particles[i]);
            }

            //Cargo la textura que tendra cada particula
            this.texture = TgcTexture.createTexture(device, texturePath);
        }

        /// <summary>
        /// Cambiar la textura de las particulas
        /// </summary>
        public void changeTexture(string texturePath)
        {
            this.texture.dispose();
            this.texture = TgcTexture.createTexture(GuiController.Instance.D3dDevice, texturePath);
        }

        /// <summary>
        /// Liberar recursos
        /// </summary>
        public void dispose()
        {
            texture.dispose();
            particlesAlive = null;
            particlesDead = null;
            particles = null;
            vertexDeclaration.Dispose();
        }

        /// <summary>
        /// Dibujar particulas.
        /// Emite particulas a medida que avanza el tiempo.
        /// </summary>
        public void render()
        {
            if (!enabled)
            {
                return;
            }

            //Ver si hay que generar nuevas particulas
            float elapsedTime = GuiController.Instance.ElapsedTime;
            tiempoAcumulado += elapsedTime;
            if (tiempoAcumulado >= this.creationFrecuency && playing)
            {
                tiempoAcumulado = 0.0f;

                //Inicializa y agrega una particula a la lista de particulas vivas.
                this.createParticle();
            }

            //Dibujar particulas existentes
            if (this.particlesAlive.Count > 0)
            {
                Device device = GuiController.Instance.D3dDevice;
                TgcTexture.Manager texturesManager = GuiController.Instance.TexturesManager;

                //Cargar VertexDeclaration
                device.VertexDeclaration = vertexDeclaration;

                //Fijo la textura actual de la particula.
                texturesManager.clear(0);
                texturesManager.clear(1);
                texturesManager.set(0, texture);
                device.Material = TgcD3dDevice.DEFAULT_MATERIAL;
                device.RenderState.AlphaBlendEnable = true;
                device.RenderState.ZBufferWriteEnable = false;
                device.Transform.World = Matrix.Identity;

                // Va recorriendo la lista de particulas vivas,
                // actualizando el tiempo de vida restante, y dibujando.
                Particle p = this.particlesAlive.peek();
                while (p != null)
                {
                    p.TimeToLive -= elapsedTime;

                    if (p.TimeToLive <= 0)
                    {
                        //Saco la particula de la lista de particulas vivas.
                        this.particlesAlive.dequeue(out p);

                        //Inserto la particula en la lista de particulas muertas.
                        this.particlesDead.Push(p);
                    }
                    else
                    {
                        //Actualizo y Dibujo la partícula
                        this.updateExistingParticle(elapsedTime, p);
                        this.renderParticle(p);
                    }

                    p = this.particlesAlive.peekNext();
                }

                //Restaurar valores de RenderState
                device.RenderState.AlphaBlendEnable = false;
                device.RenderState.ZBufferWriteEnable = true;
            }
        }

        /// <summary>
        /// Crear una nueva particula
        /// </summary>
        private void createParticle()
        {
            //Saco una particula de la lista de particulas muertas.
            if (particlesDead.Count > 0)
            {
                Particle p = particlesDead.Pop();
                //Agrego la partícula a la lista de partículas vivas.
                this.particlesAlive.enqueue(p);

                //Seteo valores iniciales de la partícula.
                p.TotalTimeToLive = this.particleTimeToLive;
                p.TimeToLive = this.particleTimeToLive;
                p.Position = position;
                p.Color = Particle.DEFAULT_COLOR.ToArgb();

                float faux;
                Vector3 pSpeed = Vector3.Empty;

                // Según la dispersion asigno una velocidad inicial. 
                //(Si la dispersion es 0 la velocidad inicial sera (0,1,0)).
                faux = random.Next(this.dispersion) / 1000.0f;
                faux *= (faux * 1000 % 2 == 0 ? 1.0f : -1.0f);
                pSpeed.X = faux * 2.0f;

                faux = 1.0f - (2.0f * random.Next(this.dispersion) / 1000.0f);
                pSpeed.Y = faux * 2.0f;

                faux = random.Next(this.dispersion) / 1000.0f;
                faux *= (faux * 1000 % 2 == 0 ? 1.0f : -1.0f);
                pSpeed.Z = faux * 2.0f;

                p.Speed = pSpeed;

                //Modifico el tamaño de manera aleatoria.
                float size = (float)random.NextDouble() * this.maxSizeParticle;
                if (size < this.minSizeParticle) size = this.minSizeParticle;
                p.PointSize = size;
            }
        }

        /// <summary>
        /// Actualizar el estado de una particula existente
        /// </summary>
        private void updateExistingParticle(float elapsedTime, Particle p)
        {
            //Actulizo posicion de la particula.
            Vector3 scaleVec = Vector3.Scale(this.speed, elapsedTime);
            scaleVec.X *= p.Speed.X;
            scaleVec.Y *= p.Speed.Y;
            scaleVec.Z *= p.Speed.Z;
            p.Position += scaleVec;
        }

        /// <summary>
        /// Dibujar particula
        /// </summary>
        private void renderParticle(Particle p)
        {
            Device device = GuiController.Instance.D3dDevice;

            //Variamos el canal Alpha de la particula con efecto Fade-in y Fade-out (hasta 20% Alpha - Normal - desde 60% alpha)
            float currentProgress = 1 - (p.TimeToLive / p.TotalTimeToLive);
            int alphaComp = 255;
            if (currentProgress < 0.2)
            {
                alphaComp = (int)((currentProgress / 0.2f) * 255f);
            }
            else if (currentProgress > 0.6)
            {
                alphaComp = (int)((1 - ((currentProgress - 0.6f) / 0.4f)) * 255);
            }
            
            //Crear nuevo color con Alpha interpolado
            Color origColor = Particle.DEFAULT_COLOR;
            p.Color = Color.FromArgb(alphaComp, origColor.R, origColor.G, origColor.B).ToArgb();

            //Render con Moduliacion de color Alpha
            int color = device.RenderState.TextureFactor;

            device.RenderState.TextureFactor = p.Color;
            device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
            device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
            device.TextureState[0].AlphaArgument2 = TextureArgument.TFactor;

            particleVertexArray[0] = p.PointSprite;
            device.DrawUserPrimitives(PrimitiveType.PointList, 1, particleVertexArray);

            //Restaurar valor original
            device.RenderState.TextureFactor = color;
        }

    }
}
