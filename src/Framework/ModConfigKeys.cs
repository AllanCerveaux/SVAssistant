using System.Runtime.Serialization;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace SVAssistant.Framework
{
	internal class ModConfigKeys
	{
		public KeybindList toogleMenu { get; set; } = new(SButton.F1);

		public void OnDeserialized(StreamingContext context)
		{
			this.toogleMenu ??= new KeybindList();
		}
	}
}
