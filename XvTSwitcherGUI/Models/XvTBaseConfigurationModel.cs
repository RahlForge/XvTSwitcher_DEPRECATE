using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace XvTSwitcherGUI.Models
{
  public class XvTBaseConfigurationModel : INotifyPropertyChanged
  {
    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    #endregion

    private string gameLaunchFolder;
    private bool hasSteamIntegration;
    private string steamLaunchFolder;

    public string GameLaunchFolder
    {
      get => gameLaunchFolder;
      set
      {
        gameLaunchFolder = value;
        OnPropertyChanged("GameLaunchFolder");
      }
    }
    
    public bool HasSteamIntegration
    {
      get => hasSteamIntegration;
      set
      {
        hasSteamIntegration = value;
        OnPropertyChanged("hasSteamIntegration");
      }
    }
    
    public string SteamLaunchFolder
    {
      get => steamLaunchFolder;
      set
      {
        steamLaunchFolder = value;
        OnPropertyChanged("SteamLaunchFolder");
      }
    }
  }
}
