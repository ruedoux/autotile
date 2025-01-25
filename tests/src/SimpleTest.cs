using System.Reflection;
using System.Diagnostics;

namespace Qwaitumin.SimpleTest;

public enum Result { SUCCESS, FAIL }

[AttributeUsage(AttributeTargets.Class)]
public class SimpleTestClass : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class SimpleTestMethod : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class SimpleBeforeEach : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class SimpleAfterEach : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class SimpleBeforeAll : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class SimpleAfterAll : Attribute { }


public record SimpleTestMethodResult(string Name, Result Result, string[] Messages);

public record SimpleTestClassResult(
  string Name,
  Result Result,
  List<SimpleTestMethodResult> MethodResults,
  long TookMiliseconds,
  string[] Messages);

// This really doesnt matter since tests will not be included in the release build,
// or it can simply be marked to not trim test classes.
#pragma warning disable IL2067
#pragma warning disable IL2026
#pragma warning disable IL2070
public class SimpleTestRunner
{
  public List<SimpleTestClassResult> ClassResults = new();

  private readonly Action<Type>? beginPrinterMethod;
  private readonly Action<SimpleTestClassResult>? resultPrinterMethod;

  public SimpleTestRunner(
    Action<Type>? beginPrinterMethod = null,
    Action<SimpleTestClassResult>? resultPrinterMethod = null)
  {
    this.beginPrinterMethod = beginPrinterMethod;
    this.resultPrinterMethod = resultPrinterMethod;
  }

  public void RunAll()
  {
    var testTypes = GetTestAllClassTypes();

    foreach (var testType in testTypes)
      RunForType(testType);
  }

  public SimpleTestClassResult RunForType(Type testClassType)
  {
    if (!testClassType.IsDefined(typeof(SimpleTestClass), inherit: true))
      throw new ArgumentException($"Passed class: '{testClassType.Name}' is not of '{nameof(SimpleTestClass)}' class type");
    beginPrinterMethod?.Invoke(testClassType);

    var testObject = Activator.CreateInstance(testClassType);
    var beforeAllMethod = GetMethodWithAttribute<SimpleBeforeAll>(testClassType);
    var afterAllMethod = GetMethodWithAttribute<SimpleAfterAll>(testClassType);
    var beforeEachMethod = GetMethodWithAttribute<SimpleBeforeEach>(testClassType);
    var afterEachMethod = GetMethodWithAttribute<SimpleAfterEach>(testClassType);
    var testMethods = GetMethodsWithAttribute<SimpleTestMethod>(testClassType);

    List<SimpleTestMethodResult> methodResults = new();
    string[] exceptionMessages = Array.Empty<string>();
    Stopwatch classStopwatch = Stopwatch.StartNew();
    try
    {
      beforeAllMethod?.Invoke(testObject, null);
      foreach (var testMethod in testMethods)
      {
        bool methodFailed = false;
        try
        {
          beforeEachMethod?.Invoke(testObject, null);
          testMethod.Invoke(testObject, null);
          afterEachMethod?.Invoke(testObject, null);
        }
        catch (Exception ex)
        {
          methodFailed = true;
          methodResults.Add(new(
            testMethod.Name,
            Result.FAIL,
            ConvertExceptionToStringArray(ex)));
        }

        if (!methodFailed)
          methodResults.Add(new(
            testMethod.Name,
            Result.SUCCESS,
            Array.Empty<string>()));
      }
      afterAllMethod?.Invoke(testObject, null);
    }
    catch (Exception ex)
    {
      exceptionMessages = ConvertExceptionToStringArray(ex);
    }
    classStopwatch.Stop();

    var result = Result.SUCCESS;
    string[] messages = new string[] { "All methods succeeded" };
    if (exceptionMessages.Length != 0)
    {
      result = Result.FAIL;
      messages = exceptionMessages;
    }
    else if (methodResults.Exists(methodResult => methodResult.Result == Result.FAIL))
    {
      result = Result.FAIL;
      messages = new string[] { "At least one of the methods has failed" };
    }

    SimpleTestClassResult simpleTestClassResult = new(
      testClassType.Name, result, methodResults, classStopwatch.ElapsedMilliseconds, messages);

    ClassResults.Add(simpleTestClassResult);
    resultPrinterMethod?.Invoke(simpleTestClassResult);
    return simpleTestClassResult;
  }

  public static Type[] GetTestAllClassTypes()
  {
    var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
    List<Type> types = new();
    foreach (var assembly in allAssemblies)
      types.AddRange(assembly.GetTypes()
        .Where(t => t.GetCustomAttributes(typeof(SimpleTestClass), true).Length > 0));

    return types.ToArray();
  }

  private static MethodInfo? GetMethodWithAttribute<T>(Type type) where T : Attribute
    => Array.Find(type.GetMethods(), m => m.GetCustomAttributes(typeof(T), true).Length > 0);

  private static MethodInfo[] GetMethodsWithAttribute<T>(Type type) where T : Attribute
    => type.GetMethods()
      .Where(m => m.GetCustomAttributes(typeof(T), true).Length > 0)
      .ToArray();

  private static string[] ConvertExceptionToStringArray(Exception ex)
    => ex.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
}
#pragma warning restore IL2067
#pragma warning restore IL2026
#pragma warning restore IL2070

public static class SimpleEqualsVerifier
{
  public static void Verify<T>(
    T obj, T objToEqual, T objToNotEqual)
    where T : notnull
  {
    Assertions.AssertEqual(obj, objToEqual);
    Assertions.AssertNotEqual(obj, objToNotEqual);
  }
}

public sealed class SimpleTestDirectory : IDisposable
{
  public readonly string AbsolutePath;

  public SimpleTestDirectory(string path = "./TEMPORARY_TEST_FOLDER")
  {
    AbsolutePath = Path.GetFullPath(path);
    Create();
  }

  public string GetRelativePath(string path)
    => $"{AbsolutePath}/{path}";

  public void Clean()
  {
    Delete();
    Create();
  }

  public void Delete()
    => Directory.Delete(AbsolutePath, recursive: true);

  private void Create()
    => Directory.CreateDirectory(AbsolutePath);

  public void Dispose()
  {
    Delete();
  }
}