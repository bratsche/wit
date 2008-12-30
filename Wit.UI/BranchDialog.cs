using System;
using Gtk;

namespace Wit.UI
{
    class BranchDialog : Dialog
    {
        public BranchDialog(string[] branches)
        {
            this.Title = "git branches";
            this.AddButton("Close", ResponseType.Close);

            branch_store = new ListStore(typeof(string));

            ScrolledWindow sw = new ScrolledWindow();
            sw.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            sw.ShadowType = ShadowType.In;

            TreeView tv = new TreeView();
            tv.Model = branch_store;
            TreeViewColumn column = tv.AppendColumn("Branch", new CellRendererText(), new TreeCellDataFunc(BranchRenderer));

            sw.Add(tv);

            base.VBox.PackStart(sw, true, true, 0);

            foreach (string s in branches)
            {
                branch_store.AppendValues(s);
            }
        }

        private ListStore branch_store;

        private void BranchRenderer(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
        {
            ((CellRendererText)cell).Text = (string)branch_store.GetValue(iter, 0);
        }
    }
}