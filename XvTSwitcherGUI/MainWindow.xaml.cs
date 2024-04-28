﻿using System.Windows;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using XvTSwitcherGUI.Installations;

namespace XvTSwitcherGUI
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private const string INSTALLATIONS_JSON = "installations.json";
    private const string BASE_GAME = "BaseGame";

    public MainWindow()
    {
      InitializeComponent();

      if (File.Exists(INSTALLATIONS_JSON) && new FileInfo(INSTALLATIONS_JSON).Length > 0)
        SourceDirectory.Text = JsonConvert.DeserializeObject<XvTInstall>(File.ReadAllText(INSTALLATIONS_JSON)).Filepath;
    }

    private void BrowseSourceDirectory_Click(object sender, RoutedEventArgs e)
    {
      var dlg = new FolderBrowserDialog();
      DialogResult result = dlg.ShowDialog();
      //DialogResult result = dlg.ShowDialog(this.GetIWin32Window());
      if (result == System.Windows.Forms.DialogResult.OK)
        SourceDirectory.Text = dlg.SelectedPath;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {     
      File.WriteAllText(INSTALLATIONS_JSON, JsonConvert.SerializeObject(new XvTInstall(BASE_GAME, SourceDirectory.Text)));
    }
  }
}
