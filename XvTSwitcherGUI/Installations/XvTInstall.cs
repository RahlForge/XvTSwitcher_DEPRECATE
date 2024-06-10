using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using XvTSwitcherGUI.ModLibrary;

namespace XvTSwitcherGUI.Installations
{
  public class XvTInstall : IXvTInstall, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private bool isActive;
    public bool IsActive
    {
      get => isActive;
      set
      {
        isActive = value;
        OnPropertyChanged("IsActive");
      }
    }

    private string name;
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

    private List<XvTModLibrary> activeModsList;
    public List<XvTModLibrary> ActiveModsList
    {
      get => activeModsList ?? new List<XvTModLibrary>();
      set
      {
        activeModsList = value;
        OnPropertyChanged("ActiveModsList");
      }
    }

    public XvTInstall() { }

    public XvTInstall(string name, string filepath)
    {
      Name = name;
      Filepath = filepath;
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
