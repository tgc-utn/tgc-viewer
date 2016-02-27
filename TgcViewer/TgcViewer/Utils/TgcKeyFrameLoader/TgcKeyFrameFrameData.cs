namespace TgcViewer.Utils.TgcKeyFrameLoader
{
    public class TgcKeyFrameFrameData
    {
        //timepo relativo al frame en el que transcurre la animacion, entre 0 y 1
        public float relativeTime;

        //Valores de cada uno de los vertices del mesh para este cuadro
        public float[] verticesCoordinates;
    }
}