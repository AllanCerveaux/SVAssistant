namespace SVAssistant.Api
{
	public class AchievementDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public bool IsUnlocked { get; set; }
	}

	internal static class Achievements
	{
		public static List<AchievementDTO> GetPlayerAchievement(StardewValley.Farmer farmer)
		{
			List<AchievementDTO> data = new List<AchievementDTO>();

			foreach (var achievement in StardewValley.Game1.achievements)
			{
				string name = achievement.Value.Split('^')[0];
				string description = achievement.Value.Split('^')[1];
				bool isUnlocked = farmer.achievements.Contains(achievement.Key);

				data.Add(
					new AchievementDTO
					{
						Id = achievement.Key,
						Name = name,
						Description = description,
						IsUnlocked = isUnlocked
					}
				);
			}

			return data;
		}
	}
}
