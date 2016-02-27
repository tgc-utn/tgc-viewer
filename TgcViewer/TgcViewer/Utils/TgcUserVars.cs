using System;
using System.Drawing;
using System.Windows.Forms;

namespace TgcViewer.Utils
{
    /// <summary>
    ///     Admnistrador de la tabla de User Vars.
    /// </summary>
    public class TgcUserVars
    {
        private const string NAME_COL = "ColumnName";
        private const string VALUE_COL = "ColumnValue";
        private readonly Color DEFAULT_VALUE_COLOR = Color.Black;

        private readonly DataGridView dataGrid;

        public TgcUserVars(DataGridView dataGrid)
        {
            this.dataGrid = dataGrid;
        }

        public string this[string varName]
        {
            set { setValue(varName, value); }
        }

        /// <summary>
        ///     Elimina todas las UserVars
        /// </summary>
        public void clearVars()
        {
            dataGrid.Rows.Clear();
        }

        /// <summary>
        ///     Agrega una nueva UserVar
        /// </summary>
        /// <param name="name">Identificador unico de la variable</param>
        public void addVar(string name)
        {
            dataGrid.Rows.Add(name, "");
        }

        /// <summary>
        ///     Carga el valor de una variable
        /// </summary>
        /// <param name="name">Identificador de la variable, cargado previamente</param>
        /// <param name="value">Valor a cargar</param>
        /// <param name="foreColor">Color de la letra</param>
        public void setValue(string name, object value, Color foreColor)
        {
            var row = findRowByVarName(name);
            if (row == null)
            {
                throw new Exception("Se intentó cargar una UserVar inexistente: " + name);
            }
            row.Cells[VALUE_COL].Value = value;
            row.Cells[VALUE_COL].Style.ForeColor = foreColor;
        }

        /// <summary>
        ///     Carga el valor de una variable
        /// </summary>
        /// <param name="name">Identificador de la variable, cargado previamente</param>
        /// <param name="value">Valor a cargar</param>
        public void setValue(string name, object value)
        {
            setValue(name, value, DEFAULT_VALUE_COLOR);
        }

        /// <summary>
        ///     Devuelve el valor de la variable especificada
        /// </summary>
        /// <param name="?">Identificador de la variable</param>
        /// <returns></returns>
        public object getValue(string name)
        {
            var row = findRowByVarName(name);
            if (row == null)
            {
                throw new Exception("Se intentó acceder una UserVar inexistente: " + name);
            }
            return row.Cells[VALUE_COL].Value;
        }

        /// <summary>
        ///     Agrega una nueva variable junto con su valor
        /// </summary>
        /// <param name="name">Identificador unico de la variable</param>
        /// <param name="value">Valor a cargar</param>
        public void addVar(string name, object value)
        {
            addVar(name);
            setValue(name, value);
        }

        private DataGridViewRow findRowByVarName(string name)
        {
            foreach (DataGridViewRow row in dataGrid.Rows)
            {
                if (row.Cells[NAME_COL].Value.Equals(name))
                {
                    return row;
                }
            }
            return null;
        }
    }
}