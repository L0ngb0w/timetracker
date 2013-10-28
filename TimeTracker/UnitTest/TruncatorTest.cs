using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TimeTracker.UnitTest
{
  [TestFixture]
  class TruncatorTest
  {
    [TestCase(Truncation.Round15, 0, 0)]
    [TestCase(Truncation.Round15, 5, 0)]
    [TestCase(Truncation.Round15, 10, 15)]
    [TestCase(Truncation.Round15, 15, 15)]
    [TestCase(Truncation.Round15, 20, 15)]
    [TestCase(Truncation.Round15, 25, 30)]
    [TestCase(Truncation.Round15, 30, 30)]
    [TestCase(Truncation.Round15, 35, 30)]
    [TestCase(Truncation.Round15, 40, 45)]
    [TestCase(Truncation.Round15, 45, 45)]
    [TestCase(Truncation.Round15, 50, 45)]
    [TestCase(Truncation.Round15, 55, 60)]
    [TestCase(Truncation.Round15, 60, 60)]
    [TestCase(Truncation.Round15, -5, 0)]
    [TestCase(Truncation.Round15, -10, -15)]
    [TestCase(Truncation.Round15, -15, -15)]
    [TestCase(Truncation.Round15, -20, -15)]
    [TestCase(Truncation.Round15, -25, -30)]
    [TestCase(Truncation.Round15, -30, -30)]
    [TestCase(Truncation.Round15, -35, -30)]
    [TestCase(Truncation.Round15, -40, -45)]
    [TestCase(Truncation.Round15, -45, -45)]
    [TestCase(Truncation.Round15, -50, -45)]
    [TestCase(Truncation.Round15, -55, -60)]
    [TestCase(Truncation.Round15, -60, -60)]
    public void Truncate_Value_TruncatedValue(Truncation algorithm, double input, double output)
    {
      var value = TimeSpan.FromMinutes(input);
      var expected = TimeSpan.FromMinutes(output);

      var actual = Truncator.Truncate(ref value, algorithm);

      Assert.AreEqual(expected, actual);
    }
  }
}
