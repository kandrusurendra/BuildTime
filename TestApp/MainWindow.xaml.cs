using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

namespace TestApp
{
    public class ProjectBuildInfo
    {
        public ProjectBuildInfo() { }

        public ProjectBuildInfo(string name, int id, DateTime startTime, bool buildSucceeded)
        {
            ProjectName = name;
            ProjectId = id;
            BuildStartTime = startTime;
            BuildSucceeded = buildSucceeded;
        }

        public string ProjectName { get; set; }

        public int ProjectId { get; set; }

        public DateTime BuildStartTime { get; set; }

        public bool BuildSucceeded { get; set; }
    }
    
    public class ViewModel
    {
        public ViewModel()
        {
            this.MyDataSource = new ObservableCollection<ProjectBuildInfo>();
            this.ViewSource = new CollectionViewSource();
            this.ViewSource.Source = this.MyDataSource;
        }

        public ObservableCollection<ProjectBuildInfo> MyDataSource { get; set; }

        public CollectionViewSource ViewSource { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new ViewModel();
            var viewModel = (this.DataContext as ViewModel);
            viewModel.MyDataSource.Add(new ProjectBuildInfo("Project1", 1, System.DateTime.Now, true));
            viewModel.MyDataSource.Add(new ProjectBuildInfo("Project2", 2, System.DateTime.Now, false));
            viewModel.MyDataSource.Add(new ProjectBuildInfo("Project3", 3, System.DateTime.Now, true));
        }

        private void OnChangeTheme(object sender, RoutedEventArgs args)
        {
            System.Console.WriteLine("OnChangeTheme called...\n");
        }
    }
}
