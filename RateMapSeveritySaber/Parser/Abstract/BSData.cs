using System;
using System.Collections.Generic;
using System.Text;

namespace RateMapSeveritySaber.Parser.Abstract;

public class BSData
{
	public List<BSNote> Notes { get; set; }
	/// <summary>Aka. Arcs</summary>
	public List<BSSlider> Sliders { get; set; }
	/// <summary>Aka. Sliders</summary>
	public List<BSBurstSlider> BurstSlider { get; set; }
	public List<BSObstacle> Obstacles { get; set; }
	public List<BSBomb> Bombs { get; set; }
}


public class BSNote { }
public class BSSlider { }
public class BSBurstSlider { }
public class BSObstacle { }
public class BSBomb { }
