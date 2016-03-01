namespace TGC.Examples.TerrainEditor.Brushes
{
    public interface ITerrainEditorBrush
    {
        /// <summary>
        ///     Accion que realiza el pincel cuando el mouse se mueve sobre el panel3D
        /// </summary>
        /// <param name="editor"></param>
        /// <returns>True si se efectuaron cambios.</returns>
        bool mouseMove(TgcTerrainEditor editor);

        /// <summary>
        ///     Accion que realiza el pincel cuando el mouse sale del panel3D.
        /// </summary>
        /// <param name="editor"></param>
        /// <returns>True si se efectuaron cambios.</returns>
        bool mouseLeave(TgcTerrainEditor editor);

        /// <summary>
        /// </summary>
        /// <param name="editor"></param>
        /// <returns>True si se efectuaron cambios.</returns>
        bool update(TgcTerrainEditor editor);

        /// <summary>
        ///     Delega la renderizacion del terreno al pincel.
        /// </summary>
        /// <param name="editor"></param>
        void render(TgcTerrainEditor editor);

        void dispose();
    }
}