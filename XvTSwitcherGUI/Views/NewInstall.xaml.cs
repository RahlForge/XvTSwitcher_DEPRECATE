using System;
using System.Windows;
using System.Windows.Forms;

namespace XvTSwitcherGUI.Windows
{
  /// <summary>
  /// Interaction logic for NewInstall.xaml
  /// </summary>
  public partial class NewInstall : Window
  {
    public NewInstall(object dataContext)
    {
      InitializeComponent();
      DataContext = dataContext;
    }

    private void NewInstallAccept_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    private void NewInstallCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Window_ContentRendered(object sender, EventArgs e)
    {
      NewInstallName.Focus();
    }

    private void BrowseExistingFolder_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new FolderBrowserDialog();
      dialog.SelectedPath = (Owner as MainWindow).GameLaunchFolder.Text;
      DialogResult result = dialog.ShowDialog();
      //DialogResult result = dlg.ShowDialog(this.GetIWin32Window());

      if (result == System.Windows.Forms.DialogResult.OK)
      {
        BrowseExistingFolder.Content = dialog.SelectedPath;

        // DetectExistingMods()
      }
    }

    //private List<XvTMod> DetectingExistingMods(string directory)
    //{
    //  var directoryInfo = new DirectoryInfo(directory);

    //  //directoryInfo.GetFiles()
    //}
  }
}
