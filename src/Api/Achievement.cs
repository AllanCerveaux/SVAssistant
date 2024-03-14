namespace SVAssistant.Api
{
	public class AchievementDTO
	{
		public int id { get; set; }
		public string name { get; set; }
		public string description { get; set; }
		public bool originalStatus { get; set; }
		public int nextId { get; set; }
		public int previousId { get; set; }
		public bool isUnlocked { get; set; }
	}

	internal static class Achievements
	{
		public static AchievementDTO AchievementsParse(
			int id,
			string achievement,
			Netcode.NetIntList famerAchivements
		)
		{
			var parts = achievement.Split('^');
			var name = parts[0].Trim();
			var description = parts[1];
			var originalStatus = bool.Parse(parts[2]);
			var previousId = int.Parse(parts[3]);
			var nextId = int.Parse(parts[4]);

			return new AchievementDTO
			{
				id = id,
				name = name,
				description = description,
				originalStatus = originalStatus,
				previousId = previousId,
				nextId = nextId,
				isUnlocked = famerAchivements.Contains(id)
			};
		}

		public static List<AchievementDTO> GetPlayerAchievement(StardewValley.Farmer farmer)
		{
			return StardewValley
				.Game1.achievements.Select(item =>
					AchievementsParse(item.Key, item.Value, farmer.achievements)
				)
				.ToList();
		}
	}
}
