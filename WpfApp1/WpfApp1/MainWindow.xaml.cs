using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;


namespace WpfApp1
{
    

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        Controllers.MainController mainController;
        string time;
        string termNum;
        string docNum;
        public IList chosenCities;

        /// <summary>
        /// MainWindow C'tor
        /// </summary>
        public MainWindow()
        {
            //laguagesD.Add("loading...", "loading...");
            InitializeComponent();
            //laguages.ItemsSource = laguagesD;
            mainController = new Controllers.MainController();
            laguages.ItemsSource = mainController.languagesD;
        }

        private void isOkEnabled()
        {
           if (path_to.Text == "" || path_from.Text == "")
            {
                start.IsEnabled = true;
            }
            else
            {
                start.IsEnabled = true;
            }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            try
            {
                FileAttributes attr = File.GetAttributes(textBox.Text);

                if (!((attr & FileAttributes.Directory) == FileAttributes.Directory))
                {
                    start.IsEnabled = false;
                }
                else
                {
                    isOkEnabled();
                }
            }
            catch
            {
                start.IsEnabled = false;
            };

        }

        private void browse_from_Click(object sender, RoutedEventArgs e)
        {
            bindButtonWithTextBox(path_from);
        }

        private void browse_to_Click(object sender, RoutedEventArgs e)
        {
            bindButtonWithTextBox(path_to);
        }
        private void bindButtonWithTextBox(System.Windows.Controls.TextBox textBox, bool isFile = false )
        {
           
            if (isFile)
            {
              var dialog = new OpenFileDialog();
                dialog.ShowDialog();
                textBox.Text = dialog.FileName;
            }
            else
            {
                var dialog = new FolderBrowserDialog();
                dialog.ShowDialog();
                textBox.Text = dialog.SelectedPath;
            }
            
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            if (path_to.Text == "" || path_from.Text == "" || !Directory.Exists(path_from.Text) || !Directory.Exists(path_to.Text))
            {
                MyMessageBox();
            }
            else
            {
                //try
                //{
                    string[] values = mainController.init(path_from.Text, path_to.Text, (bool)is_stemming.IsChecked);
                    time = "TotalTime: " + values[0] + "seconds";
                    docNum = "Number of docs: " + values[1];
                    termNum = "Number of terms: " + values[2];
                    System.Windows.Forms.MessageBox.Show(time + "\n" + docNum + "\n" + termNum, "Process Finished", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                //laguagesD = mainController.getLaguages();
                //laguages.ItemsSource = laguagesD;
                laguages.ItemsSource = mainController.languagesD;
                /*}
                catch (Exception exception)
                {
                    System.Windows.Forms.MessageBox.Show(exception.Message, "Error Occured",  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }*/
            }
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime start = DateTime.Now;
                mainController.reset(path_to.Text);
                System.Windows.Forms.MessageBox.Show("Reset process was successfully completed.\nIt took "  + (DateTime.Now - start) + ".", "Process Finished", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch(Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message, "Error Occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void load_dictionary_Click(object sender, RoutedEventArgs e)
        {
            if (path_to.Text == "")
            {
                MyMessageBox();
            }
            else
            {
                try
                {
                    DateTime start = DateTime.Now;
                    mainController.LoadDictionary(path_to.Text, (bool)is_stemming.IsChecked);
                    System.Windows.Forms.MessageBox.Show("Loading dictionary process was successfully completed.\nIt took " + (DateTime.Now - start) + ".", "Process Finished", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                catch (Exception exception)
                {
                    System.Windows.Forms.MessageBox.Show(exception.Message, "Error Occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void show_dictionary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainController.getDictionary();
                string dictionaryPath = (bool)is_stemming.IsChecked ? "\\Stemmingshow.txt": "\\show.txt";
                Process.Start(path_to.Text + dictionaryPath);
            }
            catch(Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message, "Error Occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void MyMessageBox()
        {
            var dialog = System.Windows.Forms.MessageBox.Show("Please choose the path of the folder containing the files to index and " +
                "the path of the folder for posting files.", "Wrong path detected", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
        }

        private void browse_quries_Click(object sender, RoutedEventArgs e)
        {
            bindButtonWithTextBox(source_for_queries,true);
        }

        private void run_queire_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = DateTime.Now;
            Dictionary<int, List<Tuple<string, double>>> ans = mainController.runQuerie(path_from.Text, path_to.Text, source_for_queries.Text, single_querie.Text, is_stemming.IsChecked, with_semantic.IsChecked, chosenCities);
            System.Windows.Forms.MessageBox.Show("Query process was successfully completed.\nIt took " + (DateTime.Now - start) + ".", "Process Finished", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            Dictionary<int, List<string>> dic = new Dictionary<int, List<string>>();
            foreach(int quer in ans.Keys)
            {
                dic.Add(quer, new List<string>());
                foreach(Tuple<string,double> item in ans[quer])
                {
                    dic[quer].Add(item.Item1);
                }
            }
            Window window = new ResultsWindow(dic,mainController);
            window.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (path_to.Text != "")
            {
                if (!cities.IsOpen)
                {
                    var dictionary = mainController.getCities(path_to.Text, (bool)is_stemming.IsChecked);
                    if (dictionary != null && dictionary.Count > 0)
                    {
                        citiesList.ItemsSource = (IDictionary)dictionary;
                        cities.IsOpen = true;
                    }
                    else
                    {
                        var dialog = System.Windows.Forms.MessageBox.Show("Either the corpus contains no cities, or no indexing was done yet. Please try start index, or choose a different corpus.", "Something is Missing", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
                    }
                }
                else
                {
                    this.chosenCities = citiesList.SelectedItems;
                    cities.IsOpen = false;
                }
            }
            else
            {
                MyMessageBox();
            }
        }
    }
}
