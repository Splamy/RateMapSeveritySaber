using System;
using System.Collections.Generic;
using System.Text;

namespace RateMapSeveritySaber.Parser.Abstract;

public interface ITimedObject
{
	float Beat { get; set; }
}

public interface ILocatedObject
{
	public int X { get; set; }
	public int Y { get; set; }
}
