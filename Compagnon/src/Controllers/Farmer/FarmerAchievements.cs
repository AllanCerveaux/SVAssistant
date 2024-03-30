using Microsoft.Xna.Framework;
using StardewValley;

namespace Compagnon.Controllers
{
	public class FarmerAchievementsEntity
	{
		private Farmer _farmer;

		public FarmerAchievementsEntity(Farmer farmer)
		{
			_farmer = farmer;
		}

		public List<FarmerAchievementDTO> GetAll()
		{
			return Game1.achievements.Select(item => Parse(item.Key, item.Value)).ToList();
		}

		public FarmerAchievementDTO? Get(int id)
		{
			Game1.achievements.TryGetValue(id, out var value);
			if (value != null)
			{
				return Parse(id, value);
			}
			return null;
		}

		public FarmerAchievementDTO Parse(int id, string value)
		{
			var parts = value.Split('^');
			var name = parts[0].Trim();
			var description = parts[1];
			var originalStatus = bool.Parse(parts[2]);
			var previousId = int.Parse(parts[3]);
			var nextId = int.Parse(parts[4]);
			var isUnlocked = GameStateQuery.CheckConditions(
				$"PLAYER HAS ACHIEVEMENT {_farmer} {id}"
			);

			return new FarmerAchievementDTO
			{
				id = id,
				name = name,
				description = description,
				originalStatus = originalStatus,
				previousId = previousId,
				nextId = nextId,
				isUnlocked = isUnlocked
			};
		}
	}
}
