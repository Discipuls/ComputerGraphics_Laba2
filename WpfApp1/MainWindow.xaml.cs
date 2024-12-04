using ImageMagick;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class FileData
    {
        public string Filename { get; set; }
        public string Extension { get; set; }
        public string Size { get; set; }
        public double Resolution { get; set; }
        public string ColorDepth { get; set; }
        public uint ImageCompression { get; set; }
    }

    public class ExtItem
    {
        public string Name { get; set; }
    }
    public partial class MainWindow : Window
    {
        public List<FileData> Items { get; set; }
        public List<ExtItem> ExtItems { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            ExtItems = new List<ExtItem> { new ExtItem { Name = "*" } };
            Items = new List<FileData>();
            ExtComboBox.ItemsSource = ExtItems;
            ExtComboBox.SelectedIndex = 0;
        }

        private void ProcessFileAddition(string f)
        {
            var fileInfo = new FileInfo(f);
            using (MagickImage image = new MagickImage(f))
            {
                string format = image.Format.ToString();
                if (!ExtItems.Exists(x => x.Name == format))
                {
                    ExtItems.Add(new ExtItem { Name = image.Format.ToString() });
                }

                uint quality = image.Quality;


                string size = image.Width.ToString() + "x" + image.Height.ToString();
                string colorDepth = image.DetermineBitDepth().ToString();
                string byteSize = "";
                double resolution = 0;
                if (image.Format.ToString() != "Pcx")
                {
                    var bitmap = BitmapFrame.Create(new Uri(f), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                    colorDepth = bitmap.Format.BitsPerPixel.ToString();

                    byteSize = $"{bitmap.PixelWidth}x{bitmap.PixelHeight}";
                    resolution = bitmap.DpiX;
                }




                Console.WriteLine($"Image format: {format}, Quality: {quality}");

                int nameStart = image.FileName.LastIndexOf("\\") + 1;
                int nameEnd = image.FileName.LastIndexOf(".");

                Items.Add(new FileData
                {
                    Filename = image.FileName.Substring(nameStart, nameEnd - nameStart),
                    Size = size,
                    Resolution = resolution,
                    ColorDepth = colorDepth,
                    ImageCompression = quality,
                    Extension = image.Format.ToString(),
                });

            }
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string file = "";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Multiselect = true;
            openFileDialog1.Filter = "Files|*.jpg; *.gif; *.tif; *.bmp; *.png; *.pcx";
            openFileDialog1.Title = "Select a File";

            if (openFileDialog1.ShowDialog() == true)
            {
                file = openFileDialog1.FileName;
                foreach (var f in openFileDialog1.FileNames)
                {
                    ProcessFileAddition(f);
                }                  

                //file = openFileDialog1.OpenFile().ToString();
                //openFileDialog1.Dispose();
            }
            this.list_view.ItemsSource = new ObservableCollection<FileData>(Items);
            this.ExtComboBox.ItemsSource = ExtItems;

            openFileDialog1 = null;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var column = (sender as GridViewColumnHeader);
            if (column == null)
            {
                return;
            }
            var sortBy = column.Column.Header.ToString();
            if (list_view.Items.SortDescriptions.Count > 0)
            {
                var currentSort = list_view.Items.SortDescriptions[0];
                if (currentSort.PropertyName == sortBy)
                {
                    var direction = (currentSort.Direction == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
                    list_view.Items.SortDescriptions.Clear();
                    list_view.Items.SortDescriptions.Add(new SortDescription(sortBy, direction));
                    return;
                }
            }

            list_view.Items.SortDescriptions.Clear();
            list_view.Items.SortDescriptions.Add(new SortDescription(sortBy, ListSortDirection.Ascending));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string file = "";
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            openFolderDialog.Multiselect = true;
            openFolderDialog.Title = "Select a Folder";

            if (openFolderDialog.ShowDialog() == true)
            {
                foreach (var folder in openFolderDialog.FolderNames)
                {

                    string[] extensions = { ".jpg", ".gif", ".tif", ".bmp", ".png", ".pcx" };

                    try
                    {
                        var files = Directory.GetFiles(folder)
                                             .Where(file => extensions.Contains(file.Substring(file.LastIndexOf("."))))
                                             .ToList();

                        foreach (var f in files)
                        {
                            ProcessFileAddition(f);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}");
                    }
                }
                /*file = openFolderDialog.FileName;
                foreach (var f in openFolderDialog.FileNames)
                {
                    ProcessFileAddition(f);
                }*/

                //file = openFileDialog1.OpenFile().ToString();
                //openFileDialog1.Dispose();
            }
            this.list_view.ItemsSource = new ObservableCollection<FileData>(Items);
            this.ExtComboBox.ItemsSource = ExtItems;

            openFolderDialog = null;
        }

        private ListCollectionView _view;
        public ICollectionView View
        {
            get { return this._view; }
        }
        private void ExtComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Items == null)
            {
                return;
            }
            var selectedExtension = (ExtComboBox.SelectedItem as ExtItem).Name;

            if (selectedExtension == "*")
            {
                list_view.ItemsSource = new ObservableCollection<FileData>(Items);
            }
            else
            {
                var filteredItems = Items.Where(item => item.Extension == selectedExtension).ToList();
                list_view.ItemsSource = new ObservableCollection<FileData>(filteredItems);
            }
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            Items.Clear();
            ExtItems.Clear();
            ExtItems.Add(new ExtItem { Name= "*" });
            this.list_view.ItemsSource = new ObservableCollection<FileData>(Items);
            this.ExtComboBox.ItemsSource = ExtItems;
        }
    }
}