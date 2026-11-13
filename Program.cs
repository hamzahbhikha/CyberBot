using System;
using System.Windows.Forms;

namespace CyberBot
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                DatabaseHelper.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not connect to MySQL. The task assistant needs the database.\n\n" + ex.Message,
                    "Database error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            Application.Run(new MainForm());
        }
    }
}