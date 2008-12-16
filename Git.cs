using System;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

namespace wit
{
    public class Git
    {
        public GitState State
        {
            get
            {
                if (!initialized)
                {
                    string output = String.Empty;
                    int status = RunProcess(@"C:\Program Files\Git\bin\git", "rev-parse --cdup", ref output);
                    if (status < 0)
                        state = GitState.GitNotFound;
                    else if (status > 0)
                        state = 0;
                    else if (status == 0)
                        state = GitState.InGitDirectory;
                    initialized = true;
                }

                return state;
            }
        }

        public string UserName
        {
            get
            {
                if (user_name == String.Empty)
                {
                    RunProcess(@"C:\Program Files\Git\bin\git", "config user.name", ref user_name);
                }

                return user_name;
            }
        }

        private int RunProcess(string command, string args, ref string output)
        {
            string text;

            var proc = new Process();
            proc.StartInfo.FileName = command;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.CreateNoWindow = true;

            // XXX: Whoa, no way!  Fix this shit.
            proc.StartInfo.EnvironmentVariables["HOME"] = @"C:\cygwin\home\cody";
            if (args != String.Empty)
            {
                proc.StartInfo.Arguments = args;
            }

            try
            {
                proc.Start();
                proc.WaitForExit();

                text = proc.StandardOutput.ReadToEnd();
                text += proc.StandardError.ReadToEnd();

                output = text;
            }
            catch (Win32Exception e)
            {
                return -1;
            }

            return proc.ExitCode;
        }

        private GitState state;
        private bool initialized = false;

        private string user_name = String.Empty;
    }
}