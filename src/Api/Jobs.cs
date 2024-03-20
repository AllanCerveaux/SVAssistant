using StardewValley;

namespace SVAssistant.Api
{
	public class JobsDTO
	{
		public int miner { get; set; }
		public int combat { get; set; }
		public int farmer { get; set; }
		public int fisher { get; set; }
		public int forager { get; set; }
	}

	internal static class Jobs
	{
		public static JobsDTO getPlayerJobs(Farmer player)
		{
			return new JobsDTO
			{
				miner = player.MiningLevel,
				combat = player.CombatLevel,
				fisher = player.FishingLevel,
				farmer = player.FarmingLevel,
				forager = player.ForagingLevel
			};
		}
	}
}
