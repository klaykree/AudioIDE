using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AudioIDE
{
    class CompileWav
    {
        private string CompilerLocation_;

        public CompileWav(string CompilerLocation)
        {
            CompilerLocation_ = CompilerLocation;
        }

        public void CreateExe(List<short> Data)
        {
            ProcessStartInfo StartInfo = new ProcessStartInfo();
            StartInfo.FileName = CompilerLocation_;

            string Args = String.Empty;
            foreach(short Value in Data)
            {
                Args += " " + Value;
            }

            StartInfo.Arguments = "-compile " + "test.exe";
            StartInfo.Arguments += Args;
            
            Process Compiler = Process.Start(StartInfo);
            if(Compiler.HasExited)
            {
                int ExitCode = Compiler.ExitCode;
            }
        }

        public void CreateWAV(List<short> Data)
        {
            ProcessStartInfo StartInfo = new ProcessStartInfo();
            StartInfo.FileName = CompilerLocation_;

            string Args = String.Empty;
            foreach(short Value in Data)
            {
                Args += " " + Value;
            }

            StartInfo.Arguments = "-wav " + "test.wav";
            StartInfo.Arguments += Args;

            Process Compiler = Process.Start(StartInfo);
            if(Compiler.HasExited)
            {
                int ExitCode = Compiler.ExitCode;
            }
        }
    }
}
