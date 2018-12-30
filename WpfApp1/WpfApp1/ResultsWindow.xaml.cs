using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ResultsWindow : Window
    {
        /*private Dictionary<int, List<string>> dic;
        private Controllers.MainController mainController;

        public Dictionary<int, List<string>> Dic
        {
            get { return dic; }
            set { dic = value; }
        }

        public ResultsWindow(Dictionary<int, List<string>> dictionary, Controllers.MainController mainController)
        {
            InitializeComponent();
            dic = dictionary;
            this.mainController = mainController;
            this.DataContext = this;
            Dic = new Dictionary<int, List<string>>();
            Dic = dictionary;

            int count = 0;
            foreach (List<string> lst in Dic.Values)
            {
                if (lst.Count > count)
                {

                    for (int i = count; i < lst.Count; i++)
                    {
                        DataGridTextColumn column = new DataGridTextColumn();
                        column.Header = "Doc Id " + (i + 1);
                        column.Binding = new Binding(string.Format("Value[{0}]", i));
                        dg.Columns.Add(column);
                    }
                    count = lst.Count;
                }
            }

        }

        private void dg_Selected(object sender, RoutedEventArgs e)
        {
            
            KeyValuePair<int, List<string>> row = (KeyValuePair<int, List<string>>)dg.SelectedItem;
            Dictionary<string, int> entetiesDict = mainController.getEnteties(row.Value[((DataGrid)sender).CurrentCell.Column.DisplayIndex - 1]);
            entitiesList.ItemsSource = entetiesDict;
            entities.IsOpen = true;
           
        }

        private void entities_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            entities.IsOpen = false;
        }
    }*/
        private Dictionary<int, List<string>> Dic;
        private Dictionary<int, List<Tuple<string, double>>> results;
        private System.Data.DataSet dataSet;
        private Controllers.MainController mainController;

        public ResultsWindow(Dictionary<int, List<string>> Dic, Controllers.MainController mainController, Dictionary<int, List<Tuple<string, double>>> results)
        {
            InitializeComponent();
            this.Dic = Dic;
            this.mainController = mainController;
            this.results = results;
            //loadTable(exampleTable);
        }

        public void fillIn()
        {
            DataTable resultsDataTable = new DataTable();

            DataColumn[] resutlsColumns = new DataColumn[50];
            for (int i = 0; i < 50; i++)
            {
                resutlsColumns[i] = new DataColumn((i + 1).ToString(), typeof(string));
                resutlsColumns[i].ReadOnly = true;
                resultsDataTable.Columns.Add(resutlsColumns[i]);
            }

            foreach(List<string> relevantDocs in Dic.Values)
            {
                resultsDataTable.Rows.Add(relevantDocs.ToArray());
            }

            ResultsDataGrid.ItemsSource = resultsDataTable.DefaultView;

            DataTable queriesDataTable = new DataTable();
            DataColumn query = new DataColumn("Query ID", typeof(int))
            {
                ReadOnly = true
            };
            queriesDataTable.Columns.Add(query);

            foreach(int queryID in Dic.Keys)
            {
                queriesDataTable.Rows.Add(queryID);
            }
            

            QueriesDataGrid.ItemsSource = queriesDataTable.DefaultView;

            foreach (DataGridColumn column in QueriesDataGrid.Columns)
            {
               column.Width = new DataGridLength(100.0, DataGridLengthUnitType.Pixel);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            fillIn();
        }

        private void ResultsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if(ResultsDataGrid.SelectedCells == null)
                {
                    return;
                }

                DataRowView dataRow = (DataRowView)ResultsDataGrid.SelectedItem;
                int index = ResultsDataGrid.CurrentCell.Column.DisplayIndex;
                string selectedDocID = dataRow.Row.ItemArray[index].ToString();

                Dictionary<string, int> entetiesDict = mainController.getEnteties(selectedDocID);
                string res = "Most prominent entites for docID " + selectedDocID + " are:";
                foreach (string entity in entetiesDict.Keys)
                {
                    res += ("\n" + entity + ", " + entetiesDict[entity]);
                }
                System.Windows.MessageBox.Show(res);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }            
        }

        private void MenuItem_Click_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_Click_Save(object sender, RoutedEventArgs e)
        {
            //private Dictionary<int, List<Tuple<string, double>>> results;

            string saveDestination = "";
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Title = "Save query results";
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "txt";
            saveFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
                saveDestination = saveFileDialog1.FileName;
            }

            foreach (int item in results.Keys)
            {
                using (FileStream fs = new FileStream(saveDestination, FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.ASCII))

                    {
                        foreach (var entry in results[item])
                        {
                            sw.WriteLine("{0} {1} {2} {3} {4} {5}", item, 0, entry.Item1, 1, 1.1, "a");
                        }
                    }
                }
            }
        }
    }
}
