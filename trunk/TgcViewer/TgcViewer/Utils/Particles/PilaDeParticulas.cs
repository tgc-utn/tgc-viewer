using System;

namespace TgcViewer.Utils.Particles
{
    /// <summary>
    /// Pila para almacenar particulas
    /// </summary>
    public class PilaDeParticulas
    {
        private Particle[] pila = null;
		private int i_cima;

        public PilaDeParticulas(int iMax)
		{
			pila = new Particle[iMax];
			i_cima = 0;
		}

        public bool push(Particle p)
		{
            //Esta llena la pila.
			if (i_cima == pila.Length)
				return false;

			pila[i_cima] = p;
			i_cima++;

			return true;
		}

        public bool pop(out Particle p)
		{
			if (i_cima == 0)
			{
				p = null;
				return false;
			}

			i_cima--;
            p = pila[i_cima];

			return true;
		}
    }
}
