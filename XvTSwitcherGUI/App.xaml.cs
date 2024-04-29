using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace XvTSwitcherGUI
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    /// <summary>
    /// Application Entry Point.
    /// </summary>
    [System.STAThreadAttribute()]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public static void Main()
    {
      XvTSwitcherGUI.App app = new XvTSwitcherGUI.App();
      app.InitializeComponent();
      app.Run();
    }
  }
}
