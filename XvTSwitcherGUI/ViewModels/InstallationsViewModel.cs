using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using XvTSwitcherGUI.Models;

namespace XvTSwitcherGUI.ViewModels
{
  public class InstallationsViewModel
  {
    private XvTBaseConfigurationModel BaseConfiguration = new XvTBaseConfigurationModel();   
    public string GameLaunchFolder => BaseConfiguration.GameLaunchFolder;
    public bool HasSteamIntegration => BaseConfiguration.HasSteamIntegration;
    public string SteamLaunchFolder => BaseConfiguration.SteamLaunchFolder;
    public ObservableCollection<XvTInstallModel> Installations = new ObservableCollection<XvTInstallModel>();
    public List<XvTModLibraryModel> ActiveInstallMods => Installations.FirstOrDefault(o => o.IsActive).ActiveModsList;

    public InstallationsViewModel() { }
    public InstallationsViewModel(XvTBaseConfigurationModel baseConfiguration, ObservableCollection<XvTInstallModel> installations) 
    { 
      BaseConfiguration = baseConfiguration;
      Installations = installations;
    }

    private ICommand mUpdater;
    public ICommand UpdateCommand
    {
      get => mUpdater = new Updater();
      set
      {
        mUpdater = value;
      }
    }

    private class Updater : ICommand
    {
      public event EventHandler CanExecuteChanged;

      public bool CanExecute(object parameter)
      {
        return true;
      }

      public void Execute(object parameter)
      {
         // Code implementation for execution
      }
    }
  }
}
