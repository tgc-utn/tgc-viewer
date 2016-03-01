using System;
using System.Windows.Forms;

namespace TGC.Viewer.Utils.Networking
{
    /// <summary>
    ///     Ventana para ver los clientes conectados al server
    /// </summary>
    public partial class TgcNetworkingModifierClientsDialog : Form
    {
        private readonly TgcNetworkingModifierControl networkingControl;

        public TgcNetworkingModifierClientsDialog(TgcNetworkingModifierControl networkingControl)
        {
            InitializeComponent();

            this.networkingControl = networkingControl;
        }

        /// <summary>
        ///     Se creo un nuevo server, limpiar todo lo anterior
        /// </summary>
        internal void onServerCreated()
        {
            networkingControl.selectedPlayerId = -1;
            dataGridViewConnectedClients.Rows.Clear();
            buttonDeleteClient.Enabled = false;
        }

        /// <summary>
        ///     Se elige un cliente de la tabla
        /// </summary>
        private void dataGridViewConnectedClients_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewConnectedClients.SelectedRows.Count > 0)
            {
                networkingControl.selectedPlayerId = (int)dataGridViewConnectedClients.SelectedRows[0].Cells[1].Value;
                buttonDeleteClient.Enabled = true;
            }
            else
            {
                buttonDeleteClient.Enabled = false;
            }
        }

        //Se quiere eliminar un cliente
        private void buttonDeleteClient_Click(object sender, EventArgs e)
        {
            networkingControl.deleteClient(networkingControl.selectedPlayerId);
        }

        /// <summary>
        ///     Agregar un cliente a la lista de conectados
        /// </summary>
        internal void addClient(TgcSocketClientInfo clientInfo)
        {
            dataGridViewConnectedClients.Rows.Add(dataGridViewConnectedClients.Rows.Count,
                clientInfo.PlayerId,
                clientInfo.Name,
                clientInfo.Address.ToString());

            //seleccionar el primer elemento de la tabla
            dataGridViewConnectedClients.Rows[0].Selected = true;
            dataGridViewConnectedClients_RowEnter(null, null);
        }

        /// <summary>
        ///     Eliminar un cliente conectado de la lista que se acaba de desconectar
        /// </summary>
        internal void onClientDisconnected(TgcSocketClientInfo clientInfo)
        {
            for (var i = 0; i < dataGridViewConnectedClients.Rows.Count; i++)
            {
                var rowPlayerId = (int)dataGridViewConnectedClients.Rows[i].Cells[1].Value;
                if (rowPlayerId == clientInfo.PlayerId)
                {
                    dataGridViewConnectedClients.Rows.RemoveAt(i);
                    break;
                }
            }

            //Ver que quedo en la tabla
            if (dataGridViewConnectedClients.Rows.Count > 0)
            {
                dataGridViewConnectedClients.Rows[0].Selected = true;
                dataGridViewConnectedClients_RowEnter(null, null);
            }
            else
            {
                buttonDeleteClient.Enabled = false;
            }
        }
    }
}