using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XvTSwitcherGUI.Models;

namespace XvTSwitcherGUI.Helpers
{
  public static class ConversionHelper
  {
    public static bool TryDeserializeFromFile<T>(string path, out T deserializedObject)
    {
      try
      {
        deserializedObject = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        return true;
      }
      catch
      {
        deserializedObject = default;
        return false;
      }
    }
  }
}
