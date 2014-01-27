using System;

namespace TimeTracker
{
  enum Truncation
  {
    Round15,
    Round5,
  }

  class Truncator
  {
    public static TimeSpan Truncate(ref TimeSpan value, Truncation truncation)
    {
      var seconds = value.TotalSeconds;

      switch (truncation)
      {
        case Truncation.Round5:
          return TimeSpan.FromSeconds(Round(seconds, 300));
        case Truncation.Round15:
          return TimeSpan.FromSeconds(Round(seconds, 900));
        default:
          throw new ArgumentException("Unsupported truncation", "truncation");
      }
    }

    static double Round(double seconds, int towards)
    {
      var current = (int)seconds;
      var c = current / towards * towards;

      if (seconds >= 0)
        return c + (current - c <= c + towards - current ? 0 : towards);
      else
        return c - (current - c >= c - towards - current ? 0 : towards);
    }
  }
}
