using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirHockey
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            BossClass boss = new BossClass();
            System.Windows.Application app = new System.Windows.Application();
            app.Run(boss.startWindow);
        }
    }
}
