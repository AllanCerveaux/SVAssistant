using StardewValley;

namespace SVAssistant.Api
{
	public class JobsDTO
	{
		public int Miner { get; set; }
		public int Combat { get; set; }
		public int Farmer { get; set; }
		public int Fisher { get; set; }
		public int Forager { get; set; }
	}

	internal static class Jobs
	{
		public static JobsDTO getPlayerJobs(Farmer player)
		{
			return new JobsDTO
			{
				Miner = player.MiningLevel,
				Combat = player.CombatLevel,
				Fisher = player.FishingLevel,
				Farmer = player.FarmingLevel,
				Forager = player.ForagingLevel
			};
		}
	}
}
