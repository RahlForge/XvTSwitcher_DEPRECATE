using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace XvTSwitcherGUI.Installations
{
  public class XvTInstallationList : INotifyPropertyChanged
  {   
    public ObservableCollection<XvTInstall> Installations { get; set; } = new ObservableCollection<XvTInstall>(); 

    private string activeInstallation;
    public string ActiveInstallation
    {
      get => activeInstallation;
      set
      {
        activeInstallation = value;
        OnPropertyChanged("ActiveInstallation");
      }
    }

    private string gameLaunchFolder;
    public string GameLaunchFolder
    {
      get => gameLaunchFolder;
      set
      {
        gameLaunchFolder = value;
        OnPropertyChanged("GameLaunchFolder");
      }
    }

    public bool HasInstallations => Installations.Any();
    public bool DoesInstallationExist(string name) => Installations.Any(o => o.Name == name);

    private bool hasSteamIntegration;
    public bool HasSteamIntegration
    {
      get => hasSteamIntegration;
      set
      {
        hasSteamIntegration = value;
        OnPropertyChanged("hasSteamIntegration");
      }
    }

    private string steamLaunchFolder;
    public string SteamLaunchFolder
    {
      get => steamLaunchFolder;
      set
      {
        steamLaunchFolder = value;
        OnPropertyChanged("SteamLaunchFolder");
      }
    }

    public XvTInstallationList() { }

    public void AddOrUpdate(string name, string filepath, string gogSteamFilepath)
    {
      if (Installations.Any(o => o.Name == name))
        Installations.Where(o => o.Name == name).ToList().ForEach(o => 
        { 
          o.Filepath = filepath; 
          o.SteamFilepath = gogSteamFilepath;
        });
      else
        Installations.Add(new XvTInstall(name, filepath, gogSteamFilepath));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
