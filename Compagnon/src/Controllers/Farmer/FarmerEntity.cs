using StardewValley;

namespace Compagnon.Controllers
{
	internal class FarmerEntity
	{
		private Farmer _farmer;

		public FarmerEntity(long id)
		{
			_farmer = Game1.getFarmer(id);
		}

		public FarmerDTO GetFarmerInformation()
		{
			return new FarmerDTO
			{
				name = _farmer.Name,
				money = _farmer.Money,
				total_money_earned = _farmer.totalMoneyEarned,
				gender = _farmer.Gender.ToString(),
				farm_name = _farmer.farmName.Value,
				favorite_thing = _farmer.favoriteThing.Value,
				horse_name = _farmer.horseName.Value,
				skills = GetFamerSkillWithProfessions(),
				statistics = GetFarmerStatistics(),
				achievements = new FarmerAchievementsEntity(_farmer).GetAll()
			};
		}

		/// <summary>
		/// @TODO: Improve this getter
		/// </summary>
		public FamerStatsDTO GetFarmerStatistics()
		{
			var stats = _farmer.stats;
			var mineLevel = _farmer.deepestMineLevel;

			return new FamerStatsDTO
			{
				days_played = stats.DaysPlayed,
				times_unconscious = stats.TimesUnconscious,
				times_fished = stats.TimesFished,
				average_bedtime = stats.AverageBedtime,
				mine_level = mineLevel,
				steps_taken = stats.StepsTaken,
				notes_found = stats.NotesFound,
				pieces_of_trash_recycled = stats.PiecesOfTrashRecycled,
				preserves_made = stats.PreservesMade,
				rabbit_wool_produced = stats.RabbitWoolProduced,
				dirt_hoed = stats.DirtHoed,
				stumps_chopped = stats.StumpsChopped,
				crops_shipped = stats.CropsShipped,
				seeds_sown = stats.SeedsSown,
				truffles_found = stats.TrufflesFound,
				cave_carrots_found = stats.CaveCarrotsFound,
				cooper_found = stats.CopperFound,
				iron_found = stats.IronFound,
				gold_found = stats.GoldFound,
				diamonds_found = stats.DiamondsFound,
				iridium_found = stats.IridiumFound,
				other_precious_gems_found = stats.OtherPreciousGemsFound,
				prismatic_shards_found = stats.PrismaticShardsFound,
				stone_gathered = stats.StoneGathered,
				mystic_stones_crushed = stats.MysticStonesCrushed,
				rocks_crushed = stats.RocksCrushed,
				beverage_made = stats.BeveragesMade,
				cheese_made = stats.CheeseMade,
				goat_cheese_made = stats.GoatCheeseMade,
				cow_milk_produced = stats.CowMilkProduced,
				goat_milk_produced = stats.GoatMilkProduced,
				duck_eggs_layed = stats.DuckEggsLayed,
				chicken_eggs_layed = stats.ChickenEggsLayed,
				sheep_wool_produced = stats.SheepWoolProduced,
				fish_caught = stats.FishCaught,
				geodes_cracked = stats.GeodesCracked,
				gifts_given = stats.GiftsGiven,
				good_friends = stats.GoodFriends,
				quests_completed = stats.QuestsCompleted,
				items_cooked = stats.ItemsCooked,
				items_crafted = stats.ItemsCrafted,
				items_foraged = stats.ItemsForaged,
				items_shipped = stats.ItemsShipped,
				weeds_eliminated = stats.WeedsEliminated,
				monsters_killed = stats.MonstersKilled,
				slimes_killed = stats.SlimesKilled
			};
		}

		public List<FarmerSkillDTO> GetFamerSkillWithProfessions()
		{
			List<FarmerSkillDTO> skillWithProfessions = new List<FarmerSkillDTO>();
			for (int i = 0; i <= 5; i++)
			{
				var whichSkillWithProfessions = new FarmerSkillEntity(_farmer, i);
				skillWithProfessions.Add(whichSkillWithProfessions.Get());
			}
			return skillWithProfessions;
		}
	}
}
