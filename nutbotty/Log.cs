using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nutbotty
{
    static class Log
    {

        public static void Message(string message, bool console)
        {
            //Debug.WriteLine("[" + DateTime.Now + "] " + message);
            if (console)
            {
                if (Program.nutbotty != null)
                {
                    //Console.WriteLine("[" + DateTime.Now + "] " + message);
                    Program.nutbotty.Invoke(new Action(() =>
                    {
                        Program.nutbotty.consoleBox.Text += string.Format("[{0}] {1}" + Environment.NewLine, DateTime.Now, message);
                    }));
                }
            }
        }

    }
}
