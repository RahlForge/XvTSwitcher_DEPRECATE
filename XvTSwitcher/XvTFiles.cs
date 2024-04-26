using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XvTSwitcher
{
  public enum XvTFilenameEnum
  {
    xvt_clean,
    xvt_tra,
    xvt_tie,
    xvt_hex,
    xvt_unknown
  }

  public static class XvTFiles
  {
    public static string XvTDirectory => @"C:\Program Files (x86)\GOG Galaxy\Games\Star Wars - XvT";

    public static Dictionary<string, XvTFilenameEnum> XvTFilenames = new()
    {
      { "_CLEAN", XvTFilenameEnum.xvt_clean },
      { "_TIE", XvTFilenameEnum.xvt_tie },
      { "_TRA", XvTFilenameEnum.xvt_tra },
      { "_HEX", XvTFilenameEnum.xvt_hex }
    };

    public static Dictionary<XvTFilenameEnum, string> XvTSetup = new()
    {
      { XvTFilenameEnum.xvt_clean, "Clean" },
      { XvTFilenameEnum.xvt_tie, "TIE Fighter Campaign" },
      { XvTFilenameEnum.xvt_tra, "TRA Mods" },
      { XvTFilenameEnum.xvt_hex, "The HEX Missions" },
      { XvTFilenameEnum.xvt_unknown, "Uknown setup...please check your directory." }
    };

    public static XvTFilenameEnum GetActiveSetup()
    {
      foreach (var kvp in XvTFilenames)
        if (!Directory.Exists(XvTDirectory + kvp.Key))
          return kvp.Value;

      return XvTFilenameEnum.xvt_unknown;
    }

    public static string GetSetupString(XvTFilenameEnum xvt) => XvTSetup[xvt];

    public static void ChangeSetup(XvTFilenameEnum newSetup)
    {
      Directory.Move(XvTDirectory, XvTDirectory + XvTFilenames.FirstOrDefault(x => x.Value == GetActiveSetup()).Key);
      Directory.Move(XvTDirectory + XvTFilenames.FirstOrDefault(x => x.Value == newSetup).Key, XvTDirectory);
    }
  }
}
