using System.Windows;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Collections.Generic;
using XvTSwitcherGUI.Installations;
using XvTSwitcherGUI.Windows;
using System.Linq;
using System.Windows.Input;
using System.Collections.ObjectModel;

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

    public MainWindow()
    {
      InitializeComponent();

      if (File.Exists(INSTALLATIONS_JSON) && new FileInfo(INSTALLATIONS_JSON).Length > 0)
        InstallationList = JsonConvert.DeserializeObject<XvTInstallationList>(File.ReadAllText(INSTALLATIONS_JSON));

      CreateNewInstall.IsEnabled = InstallationList.HasBaseInstallation;
      DataContext = InstallationList;
    }

    private void BrowseSourceDirectory_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new FolderBrowserDialog();
      DialogResult result = dialog.ShowDialog();
      //DialogResult result = dlg.ShowDialog(this.GetIWin32Window());

      if (result == System.Windows.Forms.DialogResult.OK)
      {
        InstallationList.BaseInstallation = new XvTInstall(string.Empty, dialog.SelectedPath);
        if (SelectActiveInstall.Text == string.Empty || InstallationList.ActiveInstallation.Name == InstallationList.BaseInstallation.Name)
          InstallationList.ActiveInstallation = InstallationList.BaseInstallation;
      }

      CreateNewInstall.IsEnabled = InstallationList.HasBaseInstallation;
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
          var newInstallPath = Path.GetFullPath($"{SourceDirectory.Text}/../{STAR_WARS_XVT} ({dialog.NewInstallName.Text})");
          var sourceDirectory = SourceDirectory.Text;

          if (Directory.Exists(newInstallPath))
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
  }
}
