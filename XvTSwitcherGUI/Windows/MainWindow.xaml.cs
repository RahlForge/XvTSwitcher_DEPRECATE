using System.Windows;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;
using XvTSwitcherGUI.Installations;
using System.Linq;
using System.Windows.Input;

namespace XvTSwitcherGUI.Windows
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private const string STAR_WARS_XVT = "Star Wars - XvT";
    private const string INSTALLATIONS_JSON = "installations.json";

    private XvTInstallationList InstallationList { get; set; } = new XvTInstallationList();
    private string PriorActiveInstallation { get; set; } = string.Empty;

    public MainWindow()
    {
      InitializeComponent();

      if (File.Exists(INSTALLATIONS_JSON) && new FileInfo(INSTALLATIONS_JSON).Length > 0)
        InstallationList = JsonConvert.DeserializeObject<XvTInstallationList>(File.ReadAllText(INSTALLATIONS_JSON));

      EnableDisableDependentControls();
      DataContext = InstallationList;
    }

    private string DefaultFilePath => Path.GetFullPath($"{SourceDirectory.Text}/../{STAR_WARS_XVT}");

    private void EnableDisableDependentControls()
    {
      var isEnabled = InstallationList.HasBaseInstallation;

      CreateNewInstall.IsEnabled = isEnabled;
      DetectExistingInstalls.IsEnabled = isEnabled;
      SelectActiveInstall.IsEnabled = isEnabled;
      RenameActiveInstall.IsEnabled = isEnabled;      
    }

    private void BrowseSourceDirectory_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new FolderBrowserDialog();
      DialogResult result = dialog.ShowDialog();
      //DialogResult result = dlg.ShowDialog(this.GetIWin32Window());

      if (result == System.Windows.Forms.DialogResult.OK)
      {
        InstallationList.CreateOrUpdateBaseGame(dialog.SelectedPath);
        if (SelectActiveInstall.Text == string.Empty)
          InstallationList.ActiveInstallation = InstallationList.BaseInstallation.Name;
      }

      EnableDisableDependentControls();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {     
      File.WriteAllText(INSTALLATIONS_JSON, JsonConvert.SerializeObject(InstallationList, Formatting.Indented));
    }

    private void CreateNewInstall_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new NewInstall();
      dialog.Owner = this;
      var result = dialog.ShowDialog() ?? false;

      if (result)
      {
        try
        {
          Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
          var newInstallPath = Path.GetFullPath($"{DefaultFilePath} ({dialog.NewInstallName.Text})");
          var sourceDirectory = SourceDirectory.Text;

          if (Directory.Exists(newInstallPath) || InstallationList.DoesInstallationExist(dialog.NewInstallName.Text))
          {
            System.Windows.MessageBox.Show("Cannot create - that installation already exists.");
            return;
          }

          CopyDirectory.IO.CopyDirectory(sourceDirectory, newInstallPath, true);
          InstallationList.AddOrUpdate(dialog.NewInstallName.Text, newInstallPath);
        }
        finally
        {
          Mouse.OverrideCursor = null;
        }
      }
    }

    private void SelectActiveInstall_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
      try
      {
        Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;                
        DirectoryInfo directory = new DirectoryInfo(DefaultFilePath);
        var newActiveInstallationPath = InstallationList.Installations.FirstOrDefault(o => o.Name == InstallationList.ActiveInstallation).Filepath;

        if (directory.FullName != newActiveInstallationPath)
        {
          var priorInstallationPath = $"{DefaultFilePath} ({PriorActiveInstallation})";
          directory.MoveTo(priorInstallationPath);
          InstallationList.AddOrUpdate(PriorActiveInstallation, priorInstallationPath);

          directory = new DirectoryInfo(newActiveInstallationPath);
          directory.MoveTo($"{DefaultFilePath}");
          InstallationList.AddOrUpdate(InstallationList.ActiveInstallation, DefaultFilePath);
        }

        PriorActiveInstallation = InstallationList.ActiveInstallation;
      }
      finally
      { 
        Mouse.OverrideCursor = null; 
      }
    }

    private void RenameActiveInstall_Click(object sender, RoutedEventArgs e)
    {
      if (InstallationList.ActiveInstallation == InstallationList.BaseInstallation.Name)
      {
        System.Windows.MessageBox.Show("Cannot rename the base installation!");
        return;
      }      

      var dialog = new NewInstall();
      dialog.Owner = this;
      var result = dialog.ShowDialog() ?? false;

      if (result)
      {
        try
        {
          SelectActiveInstall.SelectionChanged -= SelectActiveInstall_SelectionChanged;
          Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

          var newName = dialog.NewInstallName.Text;
          if (InstallationList.DoesInstallationExist(newName) && InstallationList.ActiveInstallation.Equals(newName) == false)
          {
            System.Windows.MessageBox.Show("Cannot rename - that installation already exists.");
            return;
          }

          InstallationList.Installations.FirstOrDefault(o => o.Name == InstallationList.ActiveInstallation).Name = newName;
          InstallationList.ActiveInstallation = newName;
          PriorActiveInstallation = InstallationList.ActiveInstallation;
        }
        finally
        {
          SelectActiveInstall.SelectionChanged += SelectActiveInstall_SelectionChanged;
          Mouse.OverrideCursor = null;
        }
      }
    }

    private void DetectExistingInstalls_Click(object sender, RoutedEventArgs e)
    {
      var baseDirectory = new DirectoryInfo($"{DefaultFilePath}/../");
      InstallationList.ActiveInstallation = InstallationList.BaseInstallation.Name;

      baseDirectory.EnumerateDirectories().Where(dir => dir.Name.Contains(STAR_WARS_XVT)).ToList().ForEach(dir =>
      {
        var extension = dir.Name.Replace(STAR_WARS_XVT, string.Empty).Replace("(", string.Empty).Replace(")", string.Empty).Trim();

        if (dir.Name.Equals(STAR_WARS_XVT) == false && InstallationList.Installations.Any(o => o.Name.Equals(extension)) == false)
          InstallationList.AddOrUpdate(extension, dir.FullName);
      });
    }

    private void Window_ContentRendered(object sender, System.EventArgs e)
    {
      SourceDirectory.Focus();
    }
  }
}
