using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormsGUI
{
    public partial class MatrixViewDialog : Form
    {
        public MatrixViewDialog()
        {
            InitializeComponent();
            
        }

        public MatrixViewDialog(List<List<short>> corrMatrix, List<List<short>> diffMatrix):this()
        {
            diffMatrixDataGridView.AutoGenerateColumns = true;
            diffMatrixDataGridView.DataSource = ConvertListToDataTable(diffMatrix);
            corrMatrixDataGridView.AutoGenerateColumns = true;
            corrMatrixDataGridView.DataSource = ConvertListToDataTable(corrMatrix);
            
        }

        private static List<string> ListToStringConverter (List<short> l)
        {
            return l.ConvertAll<string>(Convert.ToString);
        }

        private static DataTable ConvertListToDataTable(List<List<short>> list)
        {
            DataTable table = new DataTable();

            var strList = list.ConvertAll(ListToStringConverter);
            int columns = 0;
            foreach (var l in list)
            {
                if (l.Count > columns)
                {
                    columns = l.Count;
                }
            }
            for (int i = 0; i < columns; i++)
            {
                table.Columns.Add();
            }
            foreach (var l in strList)
            {
                table.Rows.Add(l.ToArray());
            }
            return table;
        }
    }
}
