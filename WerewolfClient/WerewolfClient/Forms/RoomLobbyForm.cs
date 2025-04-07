using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WerewolfClient.Forms
{
    public partial class RoomLobbyForm : Form
    {
        public RoomLobbyForm()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private LobbyForm _LobbyForm;

        public RoomLobbyForm(LobbyForm lobbyForm)
        {
            InitializeComponent();
            _LobbyForm = lobbyForm;
        }
        private void btnReturn_Click(object sender, EventArgs e)
       {
            _LobbyForm.Show();
            this.Close();
        }

        private void btnFind_Click(object sender, EventArgs e)
        {

        }
    }
}
