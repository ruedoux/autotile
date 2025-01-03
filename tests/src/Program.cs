using System.Diagnostics;
using Qwaitumin.GameCore;

namespace Qwaitumin.AutoTileTests;

public static class Ansi
{
  public static readonly string NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
  public static readonly string RED = Console.IsOutputRedirected ? "" : "\x1b[91m";
  public static readonly string GREEN = Console.IsOutputRedirected ? "" : "\x1b[92m";
  public static readonly string BLUE = Console.IsOutputRedirected ? "" : "\x1b[94m";
  public static readonly string GREY = Console.IsOutputRedirected ? "" : "\x1b[97m";
}

public static class Program
{
  static readonly string PREFIX_RUN = "[RUN]";
  static readonly string PREFIX_OK = "[OK ]";
  static readonly string PREFIX_ERROR = "[ERR]";

  static void Main()
  {
    SimpleTestRunner simpleTestRunner = new(LogClassBegin, LogClassResult);
    Stopwatch stopwatch = Stopwatch.StartNew();
    simpleTestRunner.RunAll();
    stopwatch.Stop();

    int classesPassed = 0, classesTotal = 0, methodsPassed = 0, methodsTotal = 0;
    foreach (var classResult in simpleTestRunner.ClassResults)
    {
      classesTotal++;
      if (classResult.Result == Result.SUCCESS) classesPassed++;
      foreach (var methodResult in classResult.MethodResults)
      {
        methodsTotal++;
        if (methodResult.Result == Result.SUCCESS) methodsPassed++;
      }
    }

    string resultText = classesTotal == classesPassed ? AddColorToString("PASS", Ansi.GREEN) : AddColorToString("FAIL", Ansi.RED);
    PrintText($"""
    ----------------------
    {resultText} Classes: {classesPassed}/{classesTotal} Methods: {methodsPassed}/{methodsTotal}
    Took {FormatTime(stopwatch.ElapsedMilliseconds)}
    ----------------------
    """);
  }

  private static void LogClassBegin(Type type)
    => PrintText($"{AddColorToString(PREFIX_RUN, Ansi.BLUE)} {type.Name}");

  private static void LogClassResult(SimpleTestClassResult classResult)
  {
    if (classResult.Result == Result.SUCCESS)
      PrintText($"{AddColorToString(PREFIX_OK, Ansi.GREEN)} {classResult.Name} {AddColorToString(FormatTime(classResult.TookMiliseconds), Ansi.GREY)}");
    if (classResult.Result == Result.FAIL)
      PrintText($"{AddColorToString(PREFIX_ERROR, Ansi.RED)} {classResult.Name} {AddColorToString(FormatTime(classResult.TookMiliseconds), Ansi.GREY)}");
    foreach (var methodResult in classResult.MethodResults)
      LogMethodResult(methodResult);
  }

  private static void LogMethodResult(SimpleTestMethodResult methodResult)
  {
    if (methodResult.Result == Result.SUCCESS)
      PrintText($"-> {AddColorToString(PREFIX_OK, Ansi.GREEN)} {methodResult.Name}");
    if (methodResult.Result == Result.FAIL)
    {
      PrintText($"->{AddColorToString(PREFIX_ERROR, Ansi.RED)} {methodResult.Name}");
      PrintText(string.Join('\n', methodResult.Messages));
    }
  }

  private static void PrintText(string text)
    => Console.WriteLine(text);

  private static string FormatTime(long miliseconds)
    => $"{miliseconds / 1000}.{miliseconds % 1000}s";

  private static string AddColorToString(string msg, string color)
    => $"{color}{msg}{Ansi.NORMAL}";
}