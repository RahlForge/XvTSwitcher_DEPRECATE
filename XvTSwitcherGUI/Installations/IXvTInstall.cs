using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XvTSwitcherGUI.Installations
{
  public interface IXvTInstall
  {
    string Name { get; set; }
    string Filepath { get; set; }
  }
}
