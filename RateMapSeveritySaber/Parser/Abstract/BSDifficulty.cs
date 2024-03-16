using RateMapSeveritySaber.Parser.Info;
using RateMapSeveritySaber.Parser.V2;

namespace RateMapSeveritySaber.Parser.Abstract;

#pragma warning disable CS8618
public class BSDifficulty
{
	public JsonInfo Info { get; set; }
	public JsonInfoMap MapInfo { get; set; }
	public BSData Data { get; set; }

	public MapCharacteristic Characteristic { get; set; }
	public int DifficultySetIndex { get; set; }
	public int DifficultyIndex { get; set; }
}
#pragma warning restore CS8618
