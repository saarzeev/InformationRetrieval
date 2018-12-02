using System;
using System.Collections;
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
        private object process;
        string time;
        string termNum;
        string docNum;

        public MainWindow()
        {
            InitializeComponent();
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
        private void bindButtonWithTextBox(System.Windows.Controls.TextBox textBox)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            textBox.Text = dialog.SelectedPath;
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            if(path_to.Text == "" || path_from.Text == "")
            {
                MyMessageBox();
            }
            else
            {
               string[] values = mainController.init(path_from.Text, path_to.Text,(bool)is_stemming.IsChecked);
                time = "TotalTime: "+ values[0];
                docNum = "Number of docs: " + values[1];
                termNum = "Number of terms: " + values[2];
                System.Windows.Forms.MessageBox.Show(time +"\n" + docNum + "\n" + termNum, "process ended!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            mainController.reset(path_to.Text);
        }

        private void load_dictionary_Click(object sender, RoutedEventArgs e)
        {
            if (path_to.Text == "")
            {
                MyMessageBox();
            }
            else
            {
                mainController.LoadDictionary(path_to.Text, (bool)is_stemming.IsChecked);
            }
        }

        private void show_dictionary_Click(object sender, RoutedEventArgs e)
        {
            mainController.getDictionary(path_to.Text, (bool)is_stemming.IsChecked);
            if(/*dictionary != null*/true)
            {
                //dictionaryList.ItemsSource = (IDictionary)dictionary;
                //dictionaryPop.IsOpen = true;
                Process.Start(path_to.Text + "\\show.txt");

            }
            else
            {
                System.Windows.Forms.MessageBox.Show("theres no dictionary to show try load or start the posting prosses first", "no dictionary", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
            }
        }

        public void MyMessageBox()
        {
            var dialog = System.Windows.Forms.MessageBox.Show("Please choose the path of the folder containing the files to index and " +
                "the path of the folder for posting files", "Missing path!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
        }
        

    }

}
