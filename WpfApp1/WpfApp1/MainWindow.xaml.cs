using System;
using System.Windows;
using System.Windows.Forms;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
            if (is_stemming.IsChecked == true)
            {
                //need to start stemmenig
            }
            //start without stemming

            //when done should pring message
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {

        }

        private void load_dictionary_Click(object sender, RoutedEventArgs e)
        {

        }

        private void show_dictionary_Click(object sender, RoutedEventArgs e)
        {

        }

        public void MyMessageBox()
        {
            var dialog = System.Windows.Forms.MessageBox.Show("please choose the path of the folder containing the files to index and " +
                "the path of the folder for posting files", "Missing path!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
        }
    }

}
