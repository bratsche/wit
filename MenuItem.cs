using System;
using System.Collections;
using System.Collections.Generic;

namespace wit
{
    public enum MenuItemFlags
    {
        ItemIsFolder
    }

    class MenuItem
    {
        public MenuItem()
        {
        }

        public MenuItem(string text, GitState state)
        {
            Text = text;
            Requisites = state;
        }

        public string Text = String.Empty;
        public string Command = String.Empty;
        public string HelpText = String.Empty;
        public GitState Requisites;
    }

    class PopupItem : MenuItem, IEnumerable<MenuItem>, IEnumerable
    {
#region IEnumerable<> implementation
        public IEnumerator<MenuItem> GetEnumerator()
        {
            foreach (MenuItem item in children)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
#endregion

        public PopupItem(string text, GitState state, MenuItem[] subitems)
            : base(text, state)
        {
            children = new List<MenuItem> (subitems);
        }

        private List<MenuItem> children;
    }
}