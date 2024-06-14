using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XvTSwitcherGUI.Models
{
  public enum XvTMods
  {
    DDrawFix,
    SixtyFpsFix
  }

  public class XvTModLibraryModel
  {
    const string MOD_LIBRARY = "./ModLibrary/";

    public XvTMods ModId { get; set; }
    public string Name { get; set; }
    public bool IsInstalled { get; set; }

    public XvTModLibraryModel() { }

    public static void CopyMod(XvTMods modEnum, string targetPath)
    {
      var modSource = $"{MOD_LIBRARY}{modEnum}";

      Directory.GetFiles(modSource).ToList().ForEach(f => {
        File.Copy(f, $"{targetPath}/{Path.GetFileName(f)}", true);
        File.Copy(f, $"{targetPath}/BalanceOfPower/{Path.GetFileName(f)}", true);
      });
    }
  }

  public static class XvTModLibraryExtensions 
  {
    public static T ToEnum<T>(this string value)
    {
      return (T)Enum.Parse(typeof(T), value, true);
    }
  }

  //public class XvTMods
  //{
  //  public Dictionary<XvTMod, bool> Mods { get; set; }
  //}
}
