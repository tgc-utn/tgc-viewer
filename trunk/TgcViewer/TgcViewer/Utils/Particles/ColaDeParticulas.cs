using System;

namespace TgcViewer.Utils.Particles
{
    /// <summary>
    /// Pila para almacenar particulas
    /// </summary>
    public class ColaDeParticulas
    {
        private Particle[] cola = null;
        private int i_frente;
        private int i_final;
        private int i_cursor = 0;

        private int count;
        /// <summary>
        /// Cantidad actual de elementos
        /// </summary>
        public int Count
        {
            get { return count; }
        }

        public ColaDeParticulas(int iMax)
		{
            cola = new Particle[iMax + 1];
			i_frente = i_final = 0;
            count = 0;
		}

        public bool enqueue(Particle p)
		{
            //La cola esta llena.
            if ((i_frente == 0 && i_final == cola.Length - 1) || (i_final == i_frente - 1)) 
                return false;

            cola[i_final] = p;
            i_final++;

            if (i_final == cola.Length) i_final = 0;

            count++;
            return true;
		}

        public bool dequeue(out Particle p)
		{
            //La cola esta vacia.
            if (i_frente == i_final)
			{
				p = null;
				return false;
			}

            p = cola[i_frente];
            i_frente++;

            if (i_frente == cola.Length) i_frente = 0;

            count--;
            return true;
		}

        public Particle peek()
		{
			if (i_frente == i_final)
				return null;

            i_cursor = i_frente;

            return cola[i_cursor];
		}

        public Particle peekNext()
		{
			i_cursor++;

			if (i_cursor == cola.Length)
				i_cursor = 0;

			if (i_cursor == i_final)
				return null;

			return cola[i_cursor];
		}
    }
}
