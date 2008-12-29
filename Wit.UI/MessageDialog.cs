using Gtk;

namespace Wit
{
    public class BaseMessageDialog
    {
        public BaseMessageDialog(Window parent, string msg, DialogFlags flags,
                                 MessageType mtype, ButtonsType btype) :
            this(parent, msg, flags, mtype, btype, false)
        {
        }

        public BaseMessageDialog(Window parent, string msg, DialogFlags flags,
                                 MessageType mtype, ButtonsType btype, bool wrap)
        {
            dialog = new MessageDialog(parent, flags, mtype, btype, GLib.Markup.EscapeText(msg));

            try {
                ((Label)((Container)((Container)dialog.VBox.Children[0]).Children[1]).Children[0]).Wrap = wrap;
            } catch { }
        }

        public int Run()
        {
            int ret = dialog.Run();
            dialog.Hide();

            return ret;
        }

        private MessageDialog dialog;
    }

    public class InfoDialog : BaseMessageDialog
    {
        public InfoDialog(Window parent, string msg, ButtonsType type) :
            base(parent, msg, DialogFlags.DestroyWithParent, MessageType.Info, type)
        {
        }

        public InfoDialog(Window parent, string msg, bool wrap) :
            base(parent, msg, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, wrap)
        {
        }
    }
}