using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WerewolfClient.Models;

namespace WerewolfClient
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // --- Dữ liệu giả để test EndGameForm ---

            string sampleResultText = "Phe Dân làng chiến thắng!"; // Hoặc "Phe Sói chiến thắng!"

            List<Player> samplePlayers = new List<Player>
            {
                new Player { Name = "Player1", Role = "Villager", IsAlive = true },
                new Player { Name = "Player2", Role = "Werewolf", IsAlive = false }, // Sói bị tiêu diệt
                new Player { Name = "Player3", Role = "Seer", IsAlive = true },
                new Player { Name = "Player4", Role = "Villager", IsAlive = false }, // Dân bị giết/treo cổ
                new Player { Name = "Player5", Role = "Villager", IsAlive = false } // Dân bị giết/treo cổ
            };

            string sampleStatsText = "Thời gian chơi: 25 phút, Số người sống sót: 2, Số sói bị diệt: 1";

            // --- Thay thế dòng Application.Run hiện tại bằng dòng này để test EndGameForm ---
            //Application.Run(new Forms.EndGameForm(sampleResultText, samplePlayers, sampleStatsText, "TEST_ROOM", false, null));
            Application.Run(new Forms.LoginForm()); 

        }
    }
}

