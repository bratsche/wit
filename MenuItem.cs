using System;

namespace wit
{
    class MenuItem
    {
        public MenuItem(string text)
        {
            Text = text;
        }

        public string Text;
        public string Command;
        public string HelpText;
    }
}