using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace DeskClock
{
    static class Program
    {
        public static DateTime EndDate = new DateTime(2015, 12, 9);
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main(String[] args) {
            if (args.Length > 0) {
                foreach (String arg in args) {
                    if (arg.StartsWith("/destday:", StringComparison.InvariantCultureIgnoreCase))
                        EndDate = DateTime.ParseExact(arg.Substring(9), "yyyy/MM/dd", null);
                }

            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
