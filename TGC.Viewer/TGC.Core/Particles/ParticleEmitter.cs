using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Textures;

namespace TGC.Core.Particles
{
    /// <summary>
    ///     Emisor de particulas para generar efectos de particulas
    /// </summary>
    public class ParticleEmitter
    {
        protected float creationFrecuency;

        protected int dispersion;

        protected bool enabled;

        protected float maxSizeParticle;

        protected float minSizeParticle;
        protected Particle[] particles;
        protected ColaDeParticulas particlesAlive;
        protected int particlesCount;
        protected Stack<Particle> particlesDead;

        protected float particleTimeToLive;
        protected Particle.ParticleVertex[] particleVertexArray;

        protected bool playing;

        protected Vector3 position;
        protected Random random;

        protected Vector3 speed;

        protected TgcTexture texture;
        protected float tiempoAcumulado;
        protected VertexDeclaration vertexDeclaration;

        /// <summary>
        ///     Crear un emisor de particulas
        /// </summary>
        /// <param name="texturePath">textura a utilizar</param>
        /// <param name="particlesCount">cantidad maxima de particlas a generar</param>
        public ParticleEmitter(string texturePath, int particlesCount)
        {
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
            vertexDeclaration = new VertexDeclaration(D3DDevice.Instance.Device, Particle.ParticleVertexElements);
            position = new Vector3(0, 0, 0);

            this.particlesCount = particlesCount;
            particles = new Particle[particlesCount];

            particlesAlive = new ColaDeParticulas(particlesCount);
            particlesDead = new Stack<Particle>(particlesCount);

            //Creo todas las particulas. Inicialmente estan todas muertas.
            for (var i = 0; i < particlesCount; i++)
            {
                particles[i] = new Particle();
                particlesDead.Push(particles[i]);
            }

            //Cargo la textura que tendra cada particula
            texture = TgcTexture.createTexture(D3DDevice.Instance.Device, texturePath);
        }

        /// <summary>
        ///     Habilita o deshabilita el emisor
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        ///     Iniciar o parar la generacion de nuevas particulas
        /// </summary>
        public bool Playing
        {
            get { return playing; }
            set { playing = value; }
        }

        /// <summary>
        ///     Posicion del emisor de particulas en la escena.
        ///     Todas las particulas se generan en esta posicion.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        ///     Tiempo de frecuencia de creacion de particulas.
        ///     Cuanto mas chico mas rapido.
        /// </summary>
        public float CreationFrecuency
        {
            get { return creationFrecuency; }
            set { creationFrecuency = value; }
        }

        /// <summary>
        ///     Tiempo de vida de las particulas.
        /// </summary>
        public float ParticleTimeToLive
        {
            get { return particleTimeToLive; }
            set { particleTimeToLive = value; }
        }

        /// <summary>
        ///     Minimo tamaño que puede tener una particula.
        /// </summary>
        public float MinSizeParticle
        {
            get { return minSizeParticle; }
            set { minSizeParticle = value; }
        }

        /// <summary>
        ///     Maximo tamaño que puede tener una particula.
        /// </summary>
        public float MaxSizeParticle
        {
            get { return maxSizeParticle; }
            set { maxSizeParticle = value; }
        }

        /// <summary>
        ///     Valor que representa el grado de dispersion de las particulas.
        /// </summary>
        public int Dispersion
        {
            get { return dispersion; }
            set { dispersion = value; }
        }

        /// <summary>
        ///     Vector que se le multiplica a la velocidad base (Por defecto es (1,1,1)).
        ///     Si Y es negativo las particulas descienden.
        /// </summary>
        public Vector3 Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        /// <summary>
        ///     Textura utilizada
        /// </summary>
        public TgcTexture Texture
        {
            get { return texture; }
        }

        /// <summary>
        ///     Cambiar la textura de las particulas
        /// </summary>
        public void changeTexture(string texturePath)
        {
            texture.dispose();
            texture = TgcTexture.createTexture(D3DDevice.Instance.Device, texturePath);
        }

        /// <summary>
        ///     Liberar recursos
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
        ///     Dibujar particulas.
        ///     Emite particulas a medida que avanza el tiempo.
        /// </summary>
        public void render(float elapsedTime)
        {
            if (!enabled)
            {
                return;
            }

            //Ver si hay que generar nuevas particulas
            tiempoAcumulado += elapsedTime;
            if (tiempoAcumulado >= creationFrecuency && playing)
            {
                tiempoAcumulado = 0.0f;

                //Inicializa y agrega una particula a la lista de particulas vivas.
                createParticle();
            }

            //Dibujar particulas existentes
            if (particlesAlive.Count > 0)
            {
                //Cargar VertexDeclaration
                D3DDevice.Instance.Device.VertexDeclaration = vertexDeclaration;

                //Fijo la textura actual de la particula.
                TexturesManager.Instance.clear(0);
                TexturesManager.Instance.clear(1);
                TexturesManager.Instance.set(0, texture);
                D3DDevice.Instance.Device.Material = D3DDevice.DEFAULT_MATERIAL;
                D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = true;
                D3DDevice.Instance.Device.RenderState.ZBufferWriteEnable = false;
                D3DDevice.Instance.Device.Transform.World = Matrix.Identity;

                // Va recorriendo la lista de particulas vivas,
                // actualizando el tiempo de vida restante, y dibujando.
                var p = particlesAlive.peek();
                while (p != null)
                {
                    p.TimeToLive -= elapsedTime;

                    if (p.TimeToLive <= 0)
                    {
                        //Saco la particula de la lista de particulas vivas.
                        particlesAlive.dequeue(out p);

                        //Inserto la particula en la lista de particulas muertas.
                        particlesDead.Push(p);
                    }
                    else
                    {
                        //Actualizo y Dibujo la partícula
                        updateExistingParticle(elapsedTime, p);
                        renderParticle(p);
                    }

                    p = particlesAlive.peekNext();
                }

                //Restaurar valores de RenderState
                D3DDevice.Instance.Device.RenderState.AlphaBlendEnable = false;
                D3DDevice.Instance.Device.RenderState.ZBufferWriteEnable = true;
            }
        }

        /// <summary>
        ///     Crear una nueva particula
        /// </summary>
        private void createParticle()
        {
            //Saco una particula de la lista de particulas muertas.
            if (particlesDead.Count > 0)
            {
                var p = particlesDead.Pop();
                //Agrego la partícula a la lista de partículas vivas.
                particlesAlive.enqueue(p);

                //Seteo valores iniciales de la partícula.
                p.TotalTimeToLive = particleTimeToLive;
                p.TimeToLive = particleTimeToLive;
                p.Position = position;
                p.Color = Particle.DEFAULT_COLOR.ToArgb();

                float faux;
                var pSpeed = Vector3.Empty;

                // Según la dispersion asigno una velocidad inicial.
                //(Si la dispersion es 0 la velocidad inicial sera (0,1,0)).
                faux = random.Next(dispersion) / 1000.0f;
                faux *= faux * 1000 % 2 == 0 ? 1.0f : -1.0f;
                pSpeed.X = faux * 2.0f;

                faux = 1.0f - 2.0f * random.Next(dispersion) / 1000.0f;
                pSpeed.Y = faux * 2.0f;

                faux = random.Next(dispersion) / 1000.0f;
                faux *= faux * 1000 % 2 == 0 ? 1.0f : -1.0f;
                pSpeed.Z = faux * 2.0f;

                p.Speed = pSpeed;

                //Modifico el tamaño de manera aleatoria.
                var size = (float)random.NextDouble() * maxSizeParticle;
                if (size < minSizeParticle) size = minSizeParticle;
                p.PointSize = size;
            }
        }

        /// <summary>
        ///     Actualizar el estado de una particula existente
        /// </summary>
        private void updateExistingParticle(float elapsedTime, Particle p)
        {
            //Actulizo posicion de la particula.
            var scaleVec = Vector3.Scale(speed, elapsedTime);
            scaleVec.X *= p.Speed.X;
            scaleVec.Y *= p.Speed.Y;
            scaleVec.Z *= p.Speed.Z;
            p.Position += scaleVec;
        }

        /// <summary>
        ///     Dibujar particula
        /// </summary>
        private void renderParticle(Particle p)
        {
            //Variamos el canal Alpha de la particula con efecto Fade-in y Fade-out (hasta 20% Alpha - Normal - desde 60% alpha)
            var currentProgress = 1 - p.TimeToLive / p.TotalTimeToLive;
            var alphaComp = 255;
            if (currentProgress < 0.2)
            {
                alphaComp = (int)(currentProgress / 0.2f * 255f);
            }
            else if (currentProgress > 0.6)
            {
                alphaComp = (int)((1 - (currentProgress - 0.6f) / 0.4f) * 255);
            }

            //Crear nuevo color con Alpha interpolado
            var origColor = Particle.DEFAULT_COLOR;
            p.Color = Color.FromArgb(alphaComp, origColor.R, origColor.G, origColor.B).ToArgb();

            //Render con Moduliacion de color Alpha
            var color = D3DDevice.Instance.Device.RenderState.TextureFactor;

            D3DDevice.Instance.Device.RenderState.TextureFactor = p.Color;
            D3DDevice.Instance.Device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
            D3DDevice.Instance.Device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
            D3DDevice.Instance.Device.TextureState[0].AlphaArgument2 = TextureArgument.TFactor;

            particleVertexArray[0] = p.PointSprite;
            D3DDevice.Instance.Device.DrawUserPrimitives(PrimitiveType.PointList, 1, particleVertexArray);

            //Restaurar valor original
            D3DDevice.Instance.Device.RenderState.TextureFactor = color;
        }
    }
}