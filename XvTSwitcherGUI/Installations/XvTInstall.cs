using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XvTSwitcherGUI.Installations
{
  public class XvTInstall : IXvTInstall
  {
    public string Name { get; set; }
    public string Filepath { get; set; }

    public XvTInstall(string name, string filepath)
    {
      Name = name;
      Filepath = filepath;
    }
  }
}
