using System.Windows;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;
using XvTSwitcherGUI.Models;
using System.Linq;
using System.Windows.Input;
using XvTSwitcherGUI.Helpers;
using XvTSwitcherGUI.ViewModels;

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
    private const string BASE_GAME = "BaseGame";

    private InstallationsViewModel ViewModel;
    private XvTInstallationList InstallationList = new XvTInstallationList();
    private XvTInstallModel ActiveInstall => InstallationList.Installations.FirstOrDefault(o => o.IsActive);

    public MainWindow()
    {
      InitializeComponent();

      if (File.Exists(INSTALLATIONS_JSON) && new FileInfo(INSTALLATIONS_JSON).Length > 0)
      {
        if (ConversionHelper.TryDeserializeFromFile(INSTALLATIONS_JSON, out InstallationList) == false)
        {
          if (ConversionHelper.TryDeserializeFromFile(INSTALLATIONS_JSON, out ViewModel) == false)
          {
            System.Windows.MessageBox.Show(this, $"Cannot read configuration from {INSTALLATIONS_JSON}", "Installations Read Error",
              MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
          }
        }
        else
          WriteInstallationListToViewModel();        
      }
      else
        ViewModel = new InstallationsViewModel();
    }

    private void WriteInstallationListToViewModel()
    {
      ViewModel = new InstallationsViewModel(
        new XvTBaseConfigurationModel()
        {
          GameLaunchFolder = InstallationList.GameLaunchFolder,
          HasSteamIntegration = InstallationList.HasSteamIntegration,
          SteamLaunchFolder = InstallationList.SteamLaunchFolder
        },
        InstallationList.Installations
      );
    }


    private bool IsCrossPlatform => HasSteamIntegration.IsChecked ?? false;

    private void EnableDisableDependentControls()
    {
      var isEnabled = string.IsNullOrEmpty(GameLaunchFolder.Text) == false;

      ActivateInstall.IsEnabled = isEnabled;
      AddInstall.IsEnabled = isEnabled;
      DetectExistingInstalls.IsEnabled = isEnabled;
      EditInstall.IsEnabled = isEnabled;   
      RemoveInstall.IsEnabled = isEnabled;
      HasSteamIntegration.IsEnabled = isEnabled;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {     
      Save();
    }

    private void AddInstall_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new NewInstall(DataContext)
      {
        Owner = this
      };

      var result = dialog.ShowDialog() ?? false;

      if (result)
      {
        try
        {
          Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
          
          var isExisting = dialog.rbSelectExisting.IsChecked ?? false;
          var installName = dialog.NewInstallName.Text;
          var installPath = isExisting ? dialog.BrowseExistingFolder.Content.ToString() : Path.GetFullPath($"{InstallationList.GameLaunchFolder} ({installName})");
          var folderName = isExisting ? new DirectoryInfo(installPath).Name : $"{InstallationList.GameLaunchFolder} ({installName})";
          var steamPath = IsCrossPlatform ? Path.GetFullPath($"{SteamDirectory.Text}") : string.Empty;
          var sourceDirectory = isExisting 
            ? installPath 
            : InstallationList.Installations.FirstOrDefault(o => o.Name == dialog.SourceFolder.Text).Filepath;

          if (isExisting == false && Directory.Exists(installPath))
          {
            System.Windows.MessageBox.Show("Cannot create - installation filepath already exists...try selecting an existing folder, instead.");
            return;
          }

          if (InstallationList.DoesInstallationExist(installName))
          {
            System.Windows.MessageBox.Show("Cannot create - installation name already in use.");
            return;
          }

          if (isExisting == false)
            CopyDirectory(sourceDirectory, installPath, true, true);

          //if (dialog.Include60FPSFix.IsChecked ?? false)
          //  CopyMod(ModLibrary.SixtyFPSFix, installPath);

          //if (dialog.IncludeDDrawFix.IsChecked ?? false)
          //  CopyMod(ModLibrary.DDrawFix, installPath);

          var newInstall = new XvTInstallModel()
          {
            Name = installName,
            Filepath = installPath
          };

          dialog.NewInstallConfigGrid.Children.OfType<System.Windows.Controls.CheckBox>().ToList().ForEach(o =>
          {
            if (o.IsChecked ?? false)
            {
              var modEnum = o.Tag.ToString().ToEnum<XvTMods>();

              XvTModLibraryModel.CopyMod(modEnum, installPath);

              var mod = new XvTModLibraryModel()
              {
                ModId = modEnum,
                Name = modEnum.ToString(),
                IsInstalled = true
              };

              newInstall.ActiveModsList.Add(mod);
            }
          });

          InstallationList.AddOrUpdate(newInstall);
        }
        finally
        {
          Save();
          Mouse.OverrideCursor = null;
        }
      }
    }

    private void ShowLoadingAnimation()
    {
      LoadingAnimation.Opacity = 1.0;
    }

    private void HideLoadingAnimation()
    {
      LoadingAnimation.Opacity = 0.0;
    }

    private void DetectExistingInstalls_Click(object sender, RoutedEventArgs e)
    {
 
    }

    private void Window_ContentRendered(object sender, System.EventArgs e)
    {     
      DataContext = InstallationList;
      EnableDisableDependentControls();

      GameLaunchFolder.Focus();
    }

    private void BrowseSteamDirectory_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new FolderBrowserDialog();

      if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        InstallationList.SteamLaunchFolder = dialog.SelectedPath;
        
        if (InstallationList.HasInstallations)
          SetActiveInstall(ActiveInstall.Filepath, InstallationList.SteamLaunchFolder);        

        Save();
      }      
    }

    private void BrowseLaunchFolder_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new FolderBrowserDialog();     

      if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        InstallationList.GameLaunchFolder = dialog.SelectedPath;

      EnableDisableDependentControls();
      Save();
    }

    public static void CopyDirectory(string sourceDir, string destinationDir, bool overwrite = false, bool recursive = true)
    {
      DirectoryInfo directoryInfo = new DirectoryInfo(sourceDir);
      if (directoryInfo.Exists == false)
      {
        throw new DirectoryNotFoundException("Source directory not found: " + directoryInfo.FullName);
      }

      DirectoryInfo[] directories = directoryInfo.GetDirectories();
      Directory.CreateDirectory(destinationDir);
      FileInfo[] files = directoryInfo.GetFiles();
      foreach (FileInfo fileInfo in files)
      {
        string destFileName = Path.Combine(destinationDir, fileInfo.Name);
        fileInfo.CopyTo(destFileName, overwrite);
      }

      if (recursive)
      {
        DirectoryInfo[] array = directories;
        foreach (DirectoryInfo directoryInfo2 in array)
        {
          string destinationDir2 = Path.Combine(destinationDir, directoryInfo2.Name);
          CopyDirectory(directoryInfo2.FullName, destinationDir2, overwrite, recursive);
        }
      }
    }

    private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      ActivateInstall_Click(sender, e);
    }

    private void ActivateInstall_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        var newActiveInstall = (XvTInstallModel)InstallationsGrid.SelectedItem;
        var confirmDialog = new ActivateInstallConfirmation()
        {
          Owner = this
        };

        confirmDialog.ActivateInstallPrompt.Content = $"Activate {newActiveInstall.Name}?";

        if (confirmDialog.ShowDialog() == true)
        {
          Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

          switch (((System.Windows.Controls.ComboBoxItem)confirmDialog.PilotProfileOption.SelectedItem).Tag.ToString())
          {
            case "Preserve":
              PreserveFiles("*.pl2", $"{ActiveInstall.Filepath}/BalanceOfPower/");
              break;
            case "Send":
              PreserveFiles("*.pl2", $"{newActiveInstall.Filepath}/BalanceOfPower/");
              break;
            case "PreserveAndSend":
              PreserveFiles("*.pl2", $"{ActiveInstall.Filepath}/BalanceOfPower/");
              PreserveFiles("*.pl2", $"{newActiveInstall.Filepath}/BalanceOfPower/");
              break;
            default:
              break;
          }

          switch (((System.Windows.Controls.ComboBoxItem)confirmDialog.PilotProfileOption.SelectedItem).Tag.ToString())
          {
            case "Preserve":
              PreserveFiles("config2.cfg", $"{ActiveInstall.Filepath}/BalanceOfPower/");
              break;
            case "Send":
              PreserveFiles("config2.cfg", $"{newActiveInstall.Filepath}/BalanceOfPower/");
              break;
            case "PreserveAndSend":
              PreserveFiles("config2.cfg", $"{ActiveInstall.Filepath}/BalanceOfPower/");
              PreserveFiles("config2.cfg", $"{newActiveInstall.Filepath}/BalanceOfPower/");
              break;
            default:
              break;
          }

          if (ActiveInstall != null)
            ActiveInstall.IsActive = false;

          InstallationList.Installations.FirstOrDefault(o => o.Name == newActiveInstall.Name).IsActive = true;

          SetActiveInstall(newActiveInstall.Filepath, InstallationList.GameLaunchFolder);          

          if (IsCrossPlatform && string.IsNullOrEmpty(InstallationList.SteamLaunchFolder) == false)
            SetActiveInstall(newActiveInstall.Filepath, InstallationList.SteamLaunchFolder);            
        }
      }
      finally
      {
        //HideLoadingAnimation();
        Save();
        Mouse.OverrideCursor = null;
      }
    }

    private void PreserveFiles(string filename, string targetDirectory)
    {
      Directory.GetFiles($"{InstallationList.GameLaunchFolder}/BalanceOfPower/", filename).ToList().ForEach(f =>
      {
        File.Copy(f, $"{targetDirectory}{Path.GetFileName(f)}", true);
      });
    }

    private void SetActiveInstall(string filepath, string targetPath)
    {
      var directory = new DirectoryInfo(targetPath);

      if (directory.Exists)
        directory.Delete(true);

      CopyDirectory(filepath, targetPath, true, true);
    }

    private void Save()
    {
      File.WriteAllText(INSTALLATIONS_JSON, JsonConvert.SerializeObject(ViewModel, Formatting.Indented));
    }

    private void RemoveInstall_Click(object sender, RoutedEventArgs e)
    {
      InstallationList.Installations.Remove((XvTInstallModel)InstallationsGrid.SelectedItem);
      Save();
    }

    private void EditInstall_Click(object sender, RoutedEventArgs e)
    {
      // TBD
      Save();
    }
  }
}
