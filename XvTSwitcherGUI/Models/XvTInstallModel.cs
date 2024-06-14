using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XvTSwitcherGUI.Models
{
  public class XvTInstallModel : INotifyPropertyChanged
  {
    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    #endregion

    private bool isActive;
    private string name;
    private string filepath;
    private List<XvTModLibraryModel> activeModsList;

    public bool IsActive
    {
      get => isActive;
      set
      {
        isActive = value;
        OnPropertyChanged("IsActive");
      }
    }
        
    public string Name 
    { 
      get => name;
      set
      {
        name = value;
        OnPropertyChanged("Name");
      } 
    }
        
    public string Filepath 
    {
      get => filepath;
      set
      {
        filepath = value;
        OnPropertyChanged("Filepath");
      } 
    }
        
    public List<XvTModLibraryModel> ActiveModsList
    {
      get
      {
        if (activeModsList == null)
          activeModsList = new List<XvTModLibraryModel>();
        return activeModsList;
      }
      set
      {
        activeModsList = value;
        OnPropertyChanged("ActiveModsList");
      }
    }

    public XvTInstallModel() { }

    public XvTInstallModel(string name, string filepath)
    {
      Name = name;
      Filepath = filepath;
    }
  }
}
