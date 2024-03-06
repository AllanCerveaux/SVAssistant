using System.Runtime.Serialization;

namespace SVAssistant.Framework
{
	internal class ModConfig
	{
		public ModConfigKeys Controls { get; set; } = new();

		public void OnDeserialized(StreamingContext context)
		{
			this.Controls ??= new ModConfigKeys();
		}
	}
}
