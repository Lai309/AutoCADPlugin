using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System.Linq;
using System.Windows.Forms;

[assembly: CommandClass(typeof(RBLayout.Commands))]

namespace RBLayout
{
    public class Commands
    {
        [CommandMethod("RBLAYOUT")]
        public void Run()
        {
            Application.ShowModelessDialog(new LayoutForm());
        }
    }

    public class LayoutForm : Form
    {
        ListBox list = new ListBox();

        public LayoutForm()
        {
            Text = "RB Layout Manager PRO";
            Width = 400;
            Height = 500;

            list.Dock = DockStyle.Fill;
            list.SelectionMode = SelectionMode.MultiExtended;
            list.AllowDrop = true;

            Controls.Add(list);

            LoadLayouts();

            list.MouseDown += (s, e) =>
            {
                if (list.SelectedItem != null)
                    list.DoDragDrop(list.SelectedItem, DragDropEffects.Move);
            };

            list.DragOver += (s, e) => e.Effect = DragDropEffects.Move;

            list.DragDrop += (s, e) =>
            {
                var point = list.PointToClient(new System.Drawing.Point(e.X, e.Y));
                int index = list.IndexFromPoint(point);

                if (index < 0) index = list.Items.Count - 1;

                object data = e.Data.GetData(typeof(string));

                list.Items.Remove(data);
                list.Items.Insert(index, data);

                ApplyOrder();
            };

            list.DoubleClick += (s, e) =>
            {
                if (list.SelectedItem == null) return;
                var doc = Application.DocumentManager.MdiActiveDocument;
                doc.SendStringToExecute($"_.LAYOUT S {list.SelectedItem} ", true, false, false);
            };
        }

        void LoadLayouts()
        {
            list.Items.Clear();

            var db = Application.DocumentManager.MdiActiveDocument.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var dict = (DBDictionary)tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead);

                foreach (DBDictionaryEntry e in dict)
                {
                    if (e.Key != "Model")
                        list.Items.Add(e.Key);
                }

                tr.Commit();
            }
        }

        void ApplyOrder()
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var dict = (DBDictionary)tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead);

                int i = 1;
                foreach (var item in list.Items)
                {
                    var layout = (Layout)tr.GetObject(dict.GetAt(item.ToString()), OpenMode.ForWrite);
                    layout.TabOrder = i++;
                }

                tr.Commit();
            }
        }
    }
}