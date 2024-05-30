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
    private const string BASE_GAME = "BaseGame";

    private XvTInstallationList InstallationList { get; set; } = new XvTInstallationList();
    private string PriorActiveInstallation { get; set; } = string.Empty;
    public XvTInstall BaseGame => InstallationList.Installations.FirstOrDefault(o => o.Name == BASE_GAME);

    public MainWindow()
    {
      InitializeComponent();

      if (File.Exists(INSTALLATIONS_JSON) && new FileInfo(INSTALLATIONS_JSON).Length > 0)
        InstallationList = JsonConvert.DeserializeObject<XvTInstallationList>(File.ReadAllText(INSTALLATIONS_JSON));      
    }

    private void SetupBaseGame()
    {
      if (System.Windows.MessageBox.Show(this, "Please set up the base installation before proceeding...", "Base installation required", 
        MessageBoxButton.OK, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
      {
        var dialog = new NewInstall(DataContext)
        {
          Title = "Select Game Launch Folder",
          Owner = this
        };

        dialog.NewInstallName.Visibility = Visibility.Collapsed;
        dialog.NewInstallNameLabel.Visibility = Visibility.Collapsed;
        dialog.rbCopyFromSource.IsChecked = false;
        dialog.rbCopyFromSource.Visibility = Visibility.Hidden;
        dialog.rbSelectExisting.IsChecked = true;
        dialog.rbSelectExisting.Visibility = Visibility.Hidden;

        if (dialog.ShowDialog() == true)
          InstallationList.GameLaunchFolder = new DirectoryInfo(dialog.BrowseExistingFolder.Content.ToString()).Name;
        else
          return;

        dialog = new NewInstall(DataContext)
        {
          Title = "Select Base Installation",
          Owner = this
        };

        dialog.NewInstallName.Text = BASE_GAME;
        dialog.NewInstallName.IsEnabled = false;
        dialog.rbCopyFromSource.IsChecked = false;
        dialog.rbCopyFromSource.Visibility = Visibility.Hidden;
        dialog.rbSelectExisting.IsChecked = true;
        dialog.rbSelectExisting.Visibility = Visibility.Hidden;        

        if (dialog.ShowDialog() == true)
        {
          var filepath = dialog.BrowseExistingFolder.Content.ToString();
          if (filepath.Contains(BASE_GAME) == false)
            filepath = $"{filepath} ({BASE_GAME})";
          InstallationList.AddOrUpdate(BASE_GAME, filepath, string.Empty);
          InstallationList.ActiveInstallation = BASE_GAME;
          PriorActiveInstallation = InstallationList.ActiveInstallation;
        }
      }
    }

    private string DefaultFilePath => BaseGame != null 
      ? Path.GetFullPath($"{BaseGame.Filepath}/../{InstallationList.GameLaunchFolder}")
      : string.Empty;

    private bool IsCrossPlatform => HasSteamIntegration.IsChecked ?? false;

    private void EnableDisableDependentControls()
    {
      var isEnabled = InstallationList.HasInstallations && string.IsNullOrEmpty(GameLaunchFolder.Text) == false;

      CreateNewInstall.IsEnabled = isEnabled;
      DetectExistingInstalls.IsEnabled = isEnabled;
      SelectActiveInstall.IsEnabled = isEnabled;
      RenameActiveInstall.IsEnabled = isEnabled;   
      HasSteamIntegration.IsEnabled = isEnabled;
    }

    private void BrowseSourceDirectory_Click(object sender, RoutedEventArgs e)
    {
    //  var dialog = new FolderBrowserDialog();
    //  DialogResult result = dialog.ShowDialog();
    //  //DialogResult result = dlg.ShowDialog(this.GetIWin32Window());

    //  if (result == System.Windows.Forms.DialogResult.OK)
    //  {
    //    InstallationList.CreateOrUpdateBaseGame(dialog.SelectedPath, IsCrossPlatform ? GOGSteamDirectory.Text : string.Empty);
    //    if (SelectActiveInstall.Text == string.Empty)
    //      InstallationList.ActiveInstallation = InstallationList.BaseInstallation.Name;
    //  }

    //  EnableDisableDependentControls();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {     
      File.WriteAllText(INSTALLATIONS_JSON, JsonConvert.SerializeObject(InstallationList, Formatting.Indented));
    }

    private void CreateNewInstall_Click(object sender, RoutedEventArgs e)
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
          var installPath = isExisting ? dialog.BrowseExistingFolder.Content.ToString() : Path.GetFullPath($"{DefaultFilePath} ({dialog.NewInstallName.Text})");
          var folderName = isExisting ? new DirectoryInfo(installPath).Name : $"{InstallationList.GameLaunchFolder} ({dialog.NewInstallName.Text})";
          var steamPath = IsCrossPlatform ? Path.GetFullPath($"{SteamDirectory.Text}") : string.Empty;
          var sourceDirectory = isExisting 
            ? installPath 
            : dialog.SourceFolder.Text == InstallationList.ActiveInstallation
              ? DefaultFilePath
              : InstallationList.Installations.FirstOrDefault(o => o.Name == dialog.SourceFolder.Text).Filepath;

          if ((isExisting == false && Directory.Exists(installPath)) || InstallationList.DoesInstallationExist(dialog.NewInstallName.Text))
          {
            System.Windows.MessageBox.Show("Cannot create - that installation already exists.");
            return;
          }

          if (isExisting == false)
            CopyDirectory(sourceDirectory, installPath, true, true);           

          if (IsCrossPlatform)
            CopyDirectory(sourceDirectory, steamPath, true, true);

          InstallationList.AddOrUpdate(dialog.NewInstallName.Text, installPath, steamPath);
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
        var newActiveInstallation = InstallationList.Installations.FirstOrDefault(o => o.Name == InstallationList.ActiveInstallation);

        if (PriorActiveInstallation != newActiveInstallation.Name)
        {
          if (PriorActiveInstallation == string.Empty)
          {
            var dialog = new NewInstall(DataContext)
            {
              Owner = this,
              Title = "Enter current installation name"
            };

            dialog.rbCopyFromSource.IsChecked = false;
            dialog.rbCopyFromSource.Visibility = Visibility.Hidden;
            dialog.rbSelectExisting.IsChecked = true;
            dialog.rbSelectExisting.Visibility = Visibility.Hidden;
            dialog.BrowseExistingFolder.Content = DefaultFilePath;
            dialog.BrowseExistingFolder.IsEnabled = false;

            if (dialog.ShowDialog() == true)
            {
              PriorActiveInstallation = dialog.NewInstallName.Text;
              InstallationList.AddOrUpdate(PriorActiveInstallation, 
                $"{DefaultFilePath} ({PriorActiveInstallation})",
                $"{InstallationList.SteamLaunchFolder} ({PriorActiveInstallation})");
            }
            else
            {
              System.Windows.MessageBox.Show("Active installation needs a name before we can switch away from it");
              return;
            }
          }

          Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
          //ShowLoadingAnimation();

          var directory = new DirectoryInfo(DefaultFilePath);
          if (directory.Exists)
            directory.Delete(true);
          CopyDirectory(newActiveInstallation.Filepath, DefaultFilePath, true, true);

          //SwapInstalls(
          //  newActiveInstallation.Filepath,
          //  InstallationList.Installations.FirstOrDefault(o => o.Name == PriorActiveInstallation).Filepath,
          //  DefaultFilePath);

          if (IsCrossPlatform && string.IsNullOrEmpty(newActiveInstallation.SteamFilepath) == false)
          {
            var steamDirectory = new DirectoryInfo(SteamDirectory.Text);
            if (steamDirectory.Exists)
              steamDirectory.Delete(true);
            CopyDirectory(newActiveInstallation.Filepath, SteamDirectory.Text, true, true);
            //SwapInstalls(
            //  newActiveInstallation.SteamFilepath,
            //  InstallationList.Installations.FirstOrDefault(o => o.Name == PriorActiveInstallation).SteamFilepath,
            //  InstallationList.SteamLaunchFolder);
          }
        }

        PriorActiveInstallation = InstallationList.ActiveInstallation;
      }
      finally
      {
        //HideLoadingAnimation();
        Mouse.OverrideCursor = null; 
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

    private void SwapInstalls(string newInstallPath, string priorInstallPath, string launchFolder)
    {
      if (newInstallPath != priorInstallPath)
      {
        new DirectoryInfo(launchFolder).MoveTo(priorInstallPath);
        new DirectoryInfo(newInstallPath).MoveTo(launchFolder);
      }
    }

    private void RenameActiveInstall_Click(object sender, RoutedEventArgs e)
    {
      if (InstallationList.ActiveInstallation == BASE_GAME)
      {
        System.Windows.MessageBox.Show("Cannot rename the base installation!");
        return;
      }

      var dialog = new NewInstall(DataContext)
      {
        Owner = this
      };

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
      InstallationList.ActiveInstallation = BASE_GAME;

      baseDirectory.EnumerateDirectories().Where(dir => dir.Name.ToLowerInvariant().Contains(InstallationList.GameLaunchFolder.ToLowerInvariant())).ToList().ForEach(dir =>
      {
        var extension = dir.Name.Replace(InstallationList.GameLaunchFolder, string.Empty).Replace("(", string.Empty).Replace(")", string.Empty).Trim();

        if (dir.Name.ToLowerInvariant().Equals(InstallationList.GameLaunchFolder.ToLowerInvariant()) == false &&
            InstallationList.Installations.Any(o => o.Name.ToLowerInvariant().Equals(extension.ToLowerInvariant())) == false)
        {
          var filepath = $"{DefaultFilePath} ({extension})";
          if (dir.FullName.Equals(filepath) == false)
            dir.MoveTo(filepath);
          //InstallationList.AddOrUpdate(extension, filepath);
        }
      });
    }

    private void Window_ContentRendered(object sender, System.EventArgs e)
    {
      if (InstallationList.HasInstallations == false)
        do
          SetupBaseGame();
        while (InstallationList.HasInstallations == false);
      else
        PriorActiveInstallation = InstallationList.ActiveInstallation;
      
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

        InstallationList.Installations.ToList().ForEach(o =>
        {
          o.SteamFilepath = $"{dialog.SelectedPath} ({o.Name})";

          var directory = new DirectoryInfo(o.SteamFilepath);
          if (directory.Exists == false && o.Name != InstallationList.ActiveInstallation)
          {
            if (System.Windows.MessageBox.Show(this, $"Need Steam directory for installation: {o.Name}. \nSelect existing? (Click 'no' to create new folder)",
              "Missing Steam directory", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
              var getSteam = new NewInstall(DataContext)
              {
                Owner = this,
                Title = "Select Steam Install"
              };

              getSteam.rbCopyFromSource.IsChecked = false;
              getSteam.rbCopyFromSource.IsEnabled = false;
              getSteam.rbSelectExisting.IsChecked = true;
              getSteam.NewInstallNameLabel.Visibility = Visibility.Collapsed;
              getSteam.NewInstallName.Visibility = Visibility.Collapsed;

              if (getSteam.ShowDialog() == true)
                o.SteamFilepath = getSteam.BrowseExistingFolder.Content.ToString();
            }
            else
              CopyDirectory(o.Filepath, directory.FullName, false, true);
          }
        });
      }
    }

    private void BrowseLaunchFolder_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new FolderBrowserDialog();     

      if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        InstallationList.GameLaunchFolder = new DirectoryInfo(dialog.SelectedPath).Name;

      EnableDisableDependentControls();
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
  }
}
