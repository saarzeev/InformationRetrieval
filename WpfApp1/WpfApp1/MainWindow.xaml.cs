using System;
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
        public Dictionary<string,string> laguagesD = new Dictionary<string, string>() ;

        public MainWindow()
        {
            laguagesD.Add("loading...", "loading...");
            InitializeComponent();
            laguages.ItemsSource = laguagesD;
            mainController = new Controllers.MainController();
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
            if(path_to.Text == "" || path_from.Text == "")
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
                    System.Windows.Forms.MessageBox.Show(time + "\n" + docNum + "\n" + termNum, "process ended!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    //laguagesD = mainController.getLaguages();
                    laguages.ItemsSource = laguagesD;
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
                mainController.reset(path_to.Text);
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
                    mainController.LoadDictionary(path_to.Text, (bool)is_stemming.IsChecked);
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
                "the path of the folder for posting files", "Missing path!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
        }

        private void browse_quries_Click(object sender, RoutedEventArgs e)
        {
            bindButtonWithTextBox(source_for_queries,true);
        }

        private void run_queire_Click(object sender, RoutedEventArgs e)
        {
           //TODO cities
            mainController.runQuerie(path_from.Text, path_to.Text, source_for_queries.Text, single_querie.Text, is_stemming.IsChecked, with_semantic.IsChecked/*,cities*/);
        }
    }

}
