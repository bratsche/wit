﻿using System;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

namespace Wit
{
    public class Git
    {
        public Git()
        {
        }

        public GitState State
        {
            get
            {
                int status = GitCommand("rev-parse --cdup");
                if (status < 0)
                    return GitState.GitNotFound;
                else if (status == 0)
                    return GitState.InGitDirectory;

                return 0;
            }
        }

        public string UserName
        {
            get
            {
                if (user_name == String.Empty)
                {
                    GitCommand("config user.name", ref user_name);
                }

                return user_name;
            }
        }

        public string Email
        {
            get
            {
                if (email == String.Empty)
                {
                    GitCommand("config user.email", ref email);
                }

                return email;
            }
        }

        public string CurrentBranch
        {
            get
            {
                if (current_branch == String.Empty)
                {
                    string output = String.Empty;
                    GitCommand("branch", ref output);

                    string[] branches = output.Split('\n');
                    foreach (string s in branches)
                    {
                        if (s.Contains("*"))
                        {
                            current_branch = s.Replace("*", String.Empty).Replace(" ", String.Empty).Replace("\n", String.Empty);
                        }
                    }
                }

                return current_branch;
            }
        }

        public void Init()
        {
            GitCommand("init");
        }

#region private methods
        private int GitCommand(string git_command)
        {
            string output = String.Empty;
            return GitCommand(git_command, ref output);
        }

        private int GitCommand(string git_command, ref string output)
        {
            return RunProcess(@"C:\Program Files\Git\bin\git", git_command, ref output);
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
            proc.StartInfo.LoadUserProfile = true;

            // XXX: We should find a better solution.
            proc.StartInfo.EnvironmentVariables["HOME"] = Environment.GetEnvironmentVariable("GIT_HOME");
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
            catch (Win32Exception)
            {
                return -1;
            }

            return proc.ExitCode;
        }
#endregion

#region private data
        private string user_name = String.Empty;
        private string email = String.Empty;
        private string current_branch = String.Empty;
#endregion
    }
}