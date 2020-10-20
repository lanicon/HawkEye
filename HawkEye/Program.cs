using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HawkEye
{
    /*
    HawkEye: Archive server content indexing tool with OCR
    Copyright (C) 2020  Torben Schweren

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

    Need to contact me?
    Email: torben.schweren@gmail.com
    Discord: Viper#3408
    */

    internal class Program
    {
        public static bool IsRunning { get; private set; }

        private static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";
            IsRunning = true;

            PrintLicense();
            Console.Write("\n\n");

            Services.Initiate();

            Services.CommandHandler.TakeControl();

            Exit();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void Exit()
        {
            Services.Dispose();
        }

        public static void Stop() => IsRunning = false;

        private static void PrintLicense()
        {
            Console.WriteLine(
                "HawkEye  Copyright (C) 2020  Torben Schweren" +
                "\nThis program comes with ABSOLUTELY NO WARRANTY!" +
                "\nThis is free software, and you are welcome to redistribute it under certain conditions." +
                "\nSee the LICENSE file for more details about warranty and conditions for redistribution." +
                "\n" +
                "\nHawkEye uses the open source Tesseract library for OCR" +
                "\nwith an open source wrapper made by Charles Weld." +
                "\nBoth are licensed under the Apache License version 2.0." +
                "\n" +
                "\nHawkEye uses the open source DocNet library for parsing PDF files" +
                "\nwith an open source wrapper made by modestas." +
                "\nBoth are licensed under the MIT License"
                );
        }
    }
}