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
    private const string BASE_GAME = "BaseGame";

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

    public XvTInstall BaseInstallation
    {
      get => Installations.FirstOrDefault(o => o.Name == BASE_GAME) ?? new XvTInstall(BASE_GAME, string.Empty);
      set
      {
        AddOrUpdate(BASE_GAME, value.Filepath);
        OnPropertyChanged("BaseInstallation");
      }
    }

    public bool HasBaseInstallation => BaseInstallation?.Filepath != string.Empty;
    public bool DoesInstallationExist(string name) => Installations.Any(o => o.Name == name);

    private bool hasGOGSteamIntegration;
    public bool HasGOGSteamIntegration
    {
      get => hasGOGSteamIntegration;
      set
      {
        hasGOGSteamIntegration = value;
        OnPropertyChanged("HasGOGSteamIntegration");
      }
    }

    public XvTInstallationList() { }

    public void AddOrUpdate(string name, string filepath, string gogSteamFilepath = "")
    {
      if (Installations.Any(o => o.Name == name))
        Installations.Where(o => o.Name == name).ToList()
          .ForEach(o => 
          {
            o.Filepath = filepath;
            o.GOGSteamFilepath = gogSteamFilepath;
          });
      else
        Installations.Add(new XvTInstall(name, filepath, gogSteamFilepath));
    }    

    public void CreateOrUpdateBaseGame(string filepath, string gogSteamFilepath = "") => BaseInstallation = new XvTInstall(BASE_GAME, filepath, gogSteamFilepath);

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
