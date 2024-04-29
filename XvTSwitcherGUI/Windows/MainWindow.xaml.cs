using System.Windows;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Collections.Generic;
using XvTSwitcherGUI.Installations;
using XvTSwitcherGUI.Windows;
using System.Linq;
using System.Windows.Input;

namespace XvTSwitcherGUI
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private const string STAR_WARS_XVT = "Star Wars - XvT";
    private const string INSTALLATIONS_JSON = "installations.json";
    private const string BASE_GAME = "BaseGame";

    private List<XvTInstall> Installations = new List<XvTInstall>();
    private XvTInstall BaseInstall => Installations.FirstOrDefault(o => o.Name == BASE_GAME);
    private bool HasBaseInstall => BaseInstall != null;

    public MainWindow()
    {
      InitializeComponent();

      if (File.Exists(INSTALLATIONS_JSON) && new FileInfo(INSTALLATIONS_JSON).Length > 0)
        Installations = JsonConvert.DeserializeObject<List<XvTInstall>>(File.ReadAllText(INSTALLATIONS_JSON));

      SourceDirectory.Text = BaseInstall?.Filepath ?? string.Empty;
      CreateNewInstall.IsEnabled = HasBaseInstall;
    }

    private void AddOrUpdateBaseInstallation()
    {
      if (HasBaseInstall)
        BaseInstall.Filepath = SourceDirectory.Text;      
      else
        Installations.Add(new XvTInstall(BASE_GAME, SourceDirectory.Text));
    }

    private void BrowseSourceDirectory_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new FolderBrowserDialog();
      DialogResult result = dialog.ShowDialog();
      //DialogResult result = dlg.ShowDialog(this.GetIWin32Window());
      if (result == System.Windows.Forms.DialogResult.OK)
      {
        SourceDirectory.Text = dialog.SelectedPath;
        AddOrUpdateBaseInstallation();
      }

      CreateNewInstall.IsEnabled = HasBaseInstall;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {     
      File.WriteAllText(INSTALLATIONS_JSON, JsonConvert.SerializeObject(Installations, Formatting.Indented));
    }

    private void CreateNewInstall_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new NewInstall();
      dialog.Owner = this;
      var result = dialog.ShowDialog() ?? false;

      if (result)
      {
        Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        var newInstallPath = Path.GetFullPath($"{SourceDirectory.Text}/../{STAR_WARS_XVT} ({dialog.NewInstallName.Text})");
        var sourceDirectory = SourceDirectory.Text;

        CopyDirectory.IO.CopyDirectory(sourceDirectory, newInstallPath, true);
        Installations.Add(new XvTInstall(dialog.NewInstallName.Text, newInstallPath));
        Mouse.OverrideCursor = null;
      }
    }
  }
}
