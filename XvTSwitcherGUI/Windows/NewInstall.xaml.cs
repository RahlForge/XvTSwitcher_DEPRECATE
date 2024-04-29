using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace XvTSwitcherGUI.Windows
{
  /// <summary>
  /// Interaction logic for NewInstall.xaml
  /// </summary>
  public partial class NewInstall : Window
  {
    public NewInstall()
    {
      InitializeComponent();
    }

    private void NewInstallAccept_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    private void NewInstallCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
  }
}
