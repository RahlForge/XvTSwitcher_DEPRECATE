using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace XvTSwitcherGUI.Installations
{
  public class XvTInstall : IXvTInstall, INotifyPropertyChanged
  {
    private string name;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name 
    { 
      get => name;
      set
      {
        name = value;
        OnPropertyChanged("Name");
      } 
    }

    private string filepath;
    public string Filepath 
    {
      get => filepath;
      set
      {
        filepath = value;
        OnPropertyChanged("Filepath");
      } 
    }

    private string gogSteamFilepath;
    public string GOGSteamFilepath
    {
      get => gogSteamFilepath;
      set
      {
        gogSteamFilepath = value;
        OnPropertyChanged("GOGSteamFilepath");
      }
    }

    public XvTInstall(string name, string filepath, string gogSteamFilepath)
    {
      Name = name;
      Filepath = filepath;
      GOGSteamFilepath = gogSteamFilepath;
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
