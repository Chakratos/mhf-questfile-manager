using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace MHF_QuestfileManager
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string pathToQuests;
        int GRPOffset = 0x164;
        string[] files;
        
        public MainWindow()
        {
            InitializeComponent();
            ReloadFiles();
        }

        private void ReloadFiles()
        {
            pathToQuests = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\quests\\";
            Directory.CreateDirectory(pathToQuests);
            files = Directory.GetFiles(pathToQuests, "*.bin").Select(file => Path.GetFileName(file)).ToArray();
            listQuestFiles.ItemsSource = files;
        }

        public void WriteToFile(string dataName, Dictionary<int, Tuple<string, string>> data, bool append = false, string filename = "")
        {
            if (filename == "") filename = dataName;
            Directory.CreateDirectory("output");
            if (append = false && File.Exists($"output\\{filename}.txt")) File.Delete($"output\\{filename}.txt");

            StreamWriter txtOutput = new StreamWriter($"output\\{filename}.txt");
            txtOutput.WriteLine($"{dataName} Values:");
            txtOutput.Write(string.Join(Environment.NewLine, data));
            txtOutput.Close();
        }

        public Dictionary<int, Tuple<string, string>> ReadAtOffset(string[] selectedFiles, int offset)
        {
            Dictionary<int, Tuple<string, string>> output = new Dictionary<int, Tuple<string, string>>();

            for (int i = 0; i < selectedFiles.Length; i++)
            {
                byte[] buffer = File.ReadAllBytes(pathToQuests + selectedFiles[i]);
                MemoryStream msInput = new MemoryStream(buffer);
                BinaryReader brInput = new BinaryReader(msInput);
                brInput.BaseStream.Seek(offset, SeekOrigin.Begin);

                output.Add(i, new Tuple<string, string>(selectedFiles[i], brInput.ReadUInt32().ToString()));
            }

            return output;
        }

        public void WriteToOffset(string[] selectedFiles, int offset, byte[] value)
        {
            for (int i = 0; i < selectedFiles.Length; i++)
            {
                byte[] buffer = File.ReadAllBytes(pathToQuests + selectedFiles[i]);

                for (int w = 0; w < value.Length; w++) buffer[offset + w] = value[w];

                // Output file
                string outputFile = pathToQuests + selectedFiles[i];
                MessageBox.Show("Wrote changes to quest file/s");
                File.WriteAllBytes(outputFile, buffer);
            }
        }

        private void readGRPBtn_Click(object sender, RoutedEventArgs e)
        {
            string[] selectedFiles = getSelectedFilesAsArray();
            if (selectedFiles.Length == 0) return;

            Dictionary<int, Tuple<string, string>> GRPValues = ReadAtOffset(selectedFiles, GRPOffset);
            WriteToFile("GRP", GRPValues);
            MessageBox.Show(string.Join(Environment.NewLine, GRPValues));
            MessageBox.Show("GRP Values for the selected files were also written to the GRP.txt");
        }

        private void setGRPBtn_Click(object sender, RoutedEventArgs e)
        {
            string[] selectedFiles = getSelectedFilesAsArray();
            if (selectedFiles.Length == 0) return;

            WriteToOffset(selectedFiles, GRPOffset, BitConverter.GetBytes(Convert.ToUInt32(GRPInput.Text)));
        }

        public string[] getSelectedFilesAsArray()
        {
            string[] selectedFiles = listQuestFiles.SelectedItems.OfType<string>().ToArray();
            if (selectedFiles.Length == 0)
            {
                MessageBox.Show("Please select at least 1 Questfile");
            }

            return selectedFiles;
        }

        private void GRPInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void reloadFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            ReloadFiles();
        }
    }
}
