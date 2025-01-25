using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Qwaitumin.SimpleTest;

public static class Assertions
{
  public static void AssertFileExists(string filePath)
  {
    if (!File.Exists(filePath))
      throw new FileNotFoundException($"File doesnt exist: {filePath}");
  }

  public static void AssertDirectoryExists(string directoryPath)
  {
    if (!Directory.Exists(directoryPath))
      throw new DirectoryNotFoundException($"Directory doesnt exist: {directoryPath}");
  }

  public static void AssertNotNull<T>([NotNull] T? obj, string additionalMessage = "")
  {
    if (obj is null)
      throw new ArgumentNullException(
        nameof(obj), $"Argument cannot be null. {additionalMessage}");
  }

  public static void AssertNull<T>(T? obj, string additionalMessage = "")
  {
    if (obj is not null)
      throw new ArgumentNullException(
        nameof(obj), $"Argument should be null. {additionalMessage}");
  }

  public static void AssertTrue(bool shoudlBeTrue, string additionalMessage = "")
  {
    if (!shoudlBeTrue)
    {
      throw new ValidationException(
        $"Value is false, but expected true. {additionalMessage}");
    }
  }

  public static void AssertFalse(bool shoudlBeFalse, string additionalMessage = "")
  {
    if (shoudlBeFalse)
    {
      throw new ValidationException(
        $"Value is true, but expected false. {additionalMessage}");
    }
  }

  public static void AssertEqual<T>(
    T shouldBe, T isNow, string additionalMessage = "")
  {
    if (!Equals(shouldBe, isNow))
    {
      throw new ValidationException(
        $"Value is not equal, is: '{isNow}', but should be: '{shouldBe}'. {additionalMessage}");
    }
  }

  public static void AssertEqual<T>(
    IEnumerable<T> shouldBe, IEnumerable<T> isNow, string additionalMessage = "")
  {
    if (!Equals(shouldBe, isNow))
    {
      throw new ValidationException(
        $"Value is not equal, is: '{isNow}', but should be: '{shouldBe}'. {additionalMessage}");
    }
  }

  public static void AssertNotEqual<T>(
    T shouldNotBe, T isNow, string additionalMessage = "")
  {
    if (Equals(shouldNotBe, isNow))
    {
      throw new ValidationException(
        $"Value is equal to: '{shouldNotBe}'. {additionalMessage}");
    }
  }

  public static void AssertLessThan<T>(
    T value, T maxValue, string additionalMessage = "")
        where T : IComparable<T>
  {
    if (value.CompareTo(maxValue) >= 0)
    {
      throw new ValidationException(
          $"Value '{value}' is not less than '{maxValue}'. {additionalMessage}");
    }
  }

  public static void AssertMoreThan<T>(
    T value, T minValue, string additionalMessage = "")
      where T : IComparable<T>
  {
    if (value.CompareTo(minValue) <= 0)
    {
      throw new ValidationException(
          $"Value '{value}' is not larger than '{minValue}'. {additionalMessage}");
    }
  }

  public static void AssertEqualOrLessThan<T>(
    T value, T maxValue, string additionalMessage = "")
        where T : IComparable<T>
  {
    if (value.CompareTo(maxValue) > 0)
    {
      throw new ValidationException(
          $"Value '{value}' is greater than '{maxValue}'. {additionalMessage}");
    }
  }

  public static void AssertEqualOrMoreThan<T>(
    T value, T minValue, string additionalMessage = "")
      where T : IComparable<T>
  {
    if (value.CompareTo(minValue) < 0)
    {
      throw new ValidationException(
          $"Value '{value}' is less than '{minValue}'. {additionalMessage}");
    }
  }

  public static void AssertNotInRange<T>(
    T value, T minValue, T maxValue, string additionalMessage = "")
      where T : IComparable<T>
  {
    if (value.CompareTo(minValue) >= 0 && value.CompareTo(maxValue) <= 0)
    {
      throw new ValidationException(
          $"Value '{value}' is in range: '{minValue}' - '{maxValue}'. {additionalMessage}");
    }
  }

  public static void AssertInRange<T>(
    T value, T minValue, T maxValue, string additionalMessage = "")
      where T : IComparable<T>
  {
    if (value.CompareTo(minValue) < 0 || value.CompareTo(maxValue) > 0)
    {
      throw new ValidationException(
          $"Value '{value}' is not in range: '{minValue}' - '{maxValue}'. {additionalMessage}");
    }
  }

  public static void AssertAwaitAtMost(long timeoutMs, Action action)
  {
    Exception trackedException = new("Empty exception");
    var actionTask = Task.Run(() =>
    {
      while (true)
      {
        try
        {
          action();
          break;
        }
        catch (Exception ex)
        {
          trackedException = ex;
        }
        Thread.Sleep(10);
      }
    });

    var timeoutTask = Task.Delay(TimeSpan.FromMilliseconds(timeoutMs));

    if (Task.WhenAny(actionTask, timeoutTask).Result == timeoutTask)
      throw new TimeoutException(
        $"Assertion was not passed in time: {timeoutMs}ms.\n" +
        $"Reason: {trackedException.Message}\n" +
        $"{trackedException.StackTrace}");

    if (actionTask.IsFaulted && actionTask.Exception != null)
      throw actionTask.Exception;
  }

  public static void AssertThrows<T>(
    Action action, string additionalMessage = "") where T : Exception
  {
    try
    {
      action();
    }
    catch (T)
    {
      return;
    }
    catch (Exception ex)
    {
      throw new ValidationException(
        $"Expected exception of type '{typeof(T)}', but got '{ex.GetType()}' instead. {additionalMessage}");
    }

    throw new ValidationException($"Expected exception of type '{typeof(T)}' was not thrown. {additionalMessage}");
  }

}