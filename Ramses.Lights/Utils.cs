using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramses.Lights;
public static class Utils
{
	public static int Threads { get; } = 4; // Environment.ProcessorCount;

	public static bool NearlyEqual(float a, float b, float epsilon = 0.001f) => Math.Abs(a - b) < epsilon;
}
