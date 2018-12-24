using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ResultsWindow : Window
    {
        private Dictionary<int, List<string>> dic;
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
    }
}
