using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace XvTSwitcherGUI.Models
{
  [Obsolete("XvTInstallationsList is deprecated. Use the InstallationsViewModel.")]
  public class XvTInstallationList : INotifyPropertyChanged 
  {
    public ObservableCollection<XvTInstallModel> Installations { get; set; } = new ObservableCollection<XvTInstallModel>(); 

    public string ActiveInstallation => Installations.FirstOrDefault(o => o.IsActive)?.Name ?? string.Empty;
    public List<string> ActiveInstallationMods => 
      Installations.FirstOrDefault(o => o.IsActive)?.ActiveModsList.Select(o => o.Name.ToString()).ToList() 
      ?? new List<string>();

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

    public void AddOrUpdate(string name, string filepath)
    {
      if (Installations.Any(o => o.Name == name))
        Installations.Where(o => o.Name == name).ToList().ForEach(o => { o.Filepath = filepath; });
      else
        Installations.Add(new XvTInstallModel(name, filepath));
    }

    public void AddOrUpdate(XvTInstallModel newInstall)
    {
      if (Installations.Any(o => o.Name == newInstall.Name || o.Filepath == newInstall.Filepath))
      {
        Installations.Where(o => o.Name == newInstall.Name || o.Filepath == newInstall.Filepath).ToList().ForEach(o =>
        {
          o = newInstall;
        });

        Installations = (ObservableCollection<XvTInstallModel>)Installations.Distinct();
      }
      else
        Installations.Add(newInstall);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
