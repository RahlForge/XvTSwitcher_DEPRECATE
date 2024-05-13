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
    //private const string STAR_WARS_XVT = "Star Wars - XvT";
    private const string STAR_WARS_XVT = "STAR WARS X-Wing vs TIE Fighter";
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
      if (string.IsNullOrEmpty(InstallationList?.ActiveInstallation ?? string.Empty) == false)
        PriorActiveInstallation = InstallationList.ActiveInstallation;
    }

    private string DefaultFilePath => Path.GetFullPath($"{SourceDirectory.Text}/../{InstallationList.GameLaunchFolder}");
    private bool IsCrossPlatform => HasGOGSteamIntegration.IsChecked ?? false;

    private void EnableDisableDependentControls()
    {
      var isEnabled = InstallationList.HasBaseInstallation;

      CreateNewInstall.IsEnabled = isEnabled;
      DetectExistingInstalls.IsEnabled = isEnabled;
      SelectActiveInstall.IsEnabled = isEnabled;
      RenameActiveInstall.IsEnabled = isEnabled;   
      HasGOGSteamIntegration.IsEnabled = isEnabled;
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

          var isExisting = dialog.rbSelectExisting.IsChecked ?? false;
          var installPath = isExisting ? dialog.BrowseExistingFolder.Content.ToString() : Path.GetFullPath($"{DefaultFilePath} ({dialog.NewInstallName.Text})");
          var folderName = isExisting ? new DirectoryInfo(installPath).Name : $"{InstallationList.GameLaunchFolder} ({dialog.NewInstallName.Text})";
          var gogSteamPath = IsCrossPlatform ? Path.GetFullPath($"{GOGSteamDirectory.Text}/{folderName}") : string.Empty;
          var sourceDirectory = isExisting ? installPath : SourceDirectory.Text;

          if ((isExisting == false && Directory.Exists(installPath)) || InstallationList.DoesInstallationExist(dialog.NewInstallName.Text))
          {
            System.Windows.MessageBox.Show("Cannot create - that installation already exists.");
            return;
          }

          if (isExisting == false)
            CopyDirectory.IO.CopyDirectory(sourceDirectory, installPath, true);           

          if (IsCrossPlatform)
            CopyDirectory.IO.CopyDirectory(sourceDirectory, gogSteamPath, true);

          InstallationList.AddOrUpdate(dialog.NewInstallName.Text, installPath, gogSteamPath);
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
          if (PriorActiveInstallation == string.Empty)
          {
            var dialog = new NewInstall();
            dialog.Owner = this;
            dialog.Title = "Enter current installation name";
            dialog.rbCopyFromSource.Visibility = Visibility.Collapsed;
            dialog.rbSelectExisting.Visibility = Visibility.Collapsed;

            if (dialog.ShowDialog() == true)
            {
              PriorActiveInstallation = dialog.NewInstallName.Text;
              InstallationList.AddOrUpdate(PriorActiveInstallation, $"{DefaultFilePath} ({PriorActiveInstallation})");
            }
            else
            {
              System.Windows.MessageBox.Show("Active installation needs a name before we can switch away from it");
              return;
            }
          }

          directory.MoveTo(InstallationList.Installations.FirstOrDefault(o => o.Name == PriorActiveInstallation).Filepath);                          

          directory = new DirectoryInfo(newActiveInstallationPath);
          directory.MoveTo($"{DefaultFilePath}");
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

      baseDirectory.EnumerateDirectories().Where(dir => dir.Name.ToLowerInvariant().Contains(InstallationList.GameLaunchFolder.ToLowerInvariant())).ToList().ForEach(dir =>
      {
        var extension = dir.Name.Replace(InstallationList.GameLaunchFolder, string.Empty).Replace("(", string.Empty).Replace(")", string.Empty).Trim();

        if (dir.Name.ToLowerInvariant().Equals(InstallationList.GameLaunchFolder.ToLowerInvariant()) == false &&
            InstallationList.Installations.Any(o => o.Name.ToLowerInvariant().Equals(extension.ToLowerInvariant())) == false)
        {
          var filepath = $"{DefaultFilePath} ({extension})";
          if (dir.FullName.Equals(filepath) == false)
            dir.MoveTo(filepath);
          InstallationList.AddOrUpdate(extension, filepath);
        }
      });
    }

    private void Window_ContentRendered(object sender, System.EventArgs e)
    {
      SourceDirectory.Focus();
    }

    private void BrowseGOGSteamDirectory_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new FolderBrowserDialog();

      if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        GOGSteamDirectory.Text = dialog.SelectedPath;
    }

    private void BrowseLaunchFolder_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new FolderBrowserDialog();     

      if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        InstallationList.GameLaunchFolder = new DirectoryInfo(dialog.SelectedPath).Name;
    }
  }
}
