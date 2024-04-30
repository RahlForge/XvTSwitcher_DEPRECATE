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

    private XvTInstall activeInstallation;    
    public XvTInstall ActiveInstallation
    {
      get => activeInstallation;
      set
      {
        activeInstallation = value;
        OnPropertyChanged("ActiveInstallation");
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

    public bool HasBaseInstallation => BaseInstallation != null;

    public XvTInstallationList() { }

    public void AddOrUpdate(string name, string filepath)
    {
      if (Installations.Any(o => o.Name == name))
        Installations.Where(o => o.Name == name).ToList().ForEach(o => o.Filepath = filepath);
      else
        Installations.Add(new XvTInstall(name, filepath));
    }    

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
