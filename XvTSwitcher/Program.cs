using System.Diagnostics;
using XvTSwitcher;

var currentSetup = XvTFiles.GetActiveSetup();

Console.WriteLine($"Current XvT setup is: {XvTFiles.GetSetupString(currentSetup)}");

Console.WriteLine("Setup options:");

var setupOptions = new List<XvTFilenameEnum>();

foreach (var filename in XvTFiles.XvTFilenames)
{
  if (currentSetup != filename.Value)
  {
    setupOptions.Add(filename.Value);
    Console.WriteLine($"{setupOptions.Count} - {XvTFiles.GetSetupString(filename.Value)}");
  }
}

Console.Write("Select a new setup: ");

var newSetupInt = int.Parse(Console.ReadLine());
var newSetup = setupOptions[newSetupInt - 1];

XvTFiles.ChangeSetup(newSetup);
