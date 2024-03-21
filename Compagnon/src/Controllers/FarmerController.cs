using System.IdentityModel.Tokens.Jwt;
using HttpServer.Framework;
using HttpServer.Framework.Decorator;
using StardewValley;

namespace Compagnon.Controllers
{
	internal class FarmerDTO
	{
		public string name { get; internal set; }
		public int money { get; internal set; }
		public uint total_money_earned { get; internal set; }
		public string gender { get; internal set; }
		public string farm_name { get; internal set; }
		public string favorite_thing { get; internal set; }
		public string horse_name { get; internal set; }
	}

	internal class FamerStatsDTO
	{
		public uint days_played { get; internal set; }
		public uint times_unconscious { get; internal set; }
		public uint times_fished { get; internal set; }
		public uint average_bedtime { get; internal set; }
		public int mine_level { get; internal set; }
		public uint steps_taken { get; internal set; }
		public uint notes_found { get; internal set; }
		public uint pieces_of_trash_recycled { get; internal set; }
		public uint preserves_made { get; internal set; }
		public uint rabbit_wool_produced { get; internal set; }
		public uint dirt_hoed { get; internal set; }
		public uint stumps_chopped { get; internal set; }
		public uint crops_shipped { get; internal set; }
		public uint seeds_sown { get; internal set; }
		public uint truffles_found { get; internal set; }
		public uint cave_carrots_found { get; internal set; }
		public uint cooper_found { get; internal set; }
		public uint iron_found { get; internal set; }
		public uint gold_found { get; internal set; }
		public uint diamonds_found { get; internal set; }
		public uint iridium_found { get; internal set; }
		public uint other_precious_gems_found { get; internal set; }
		public uint prismatic_shards_found { get; internal set; }
		public uint stone_gathered { get; internal set; }
		public uint mystic_stones_crushed { get; internal set; }
		public uint rocks_crushed { get; internal set; }
		public uint beverage_made { get; internal set; }
		public uint cheese_made { get; internal set; }
		public uint goat_cheese_made { get; internal set; }
		public uint cow_milk_produced { get; internal set; }
		public uint goat_milk_produced { get; internal set; }
		public uint duck_eggs_layed { get; internal set; }
		public uint chicken_eggs_layed { get; internal set; }
		public uint sheep_wool_produced { get; internal set; }
		public uint fish_caught { get; internal set; }
		public uint geodes_cracked { get; internal set; }
		public uint gifts_given { get; internal set; }
		public uint good_friends { get; internal set; }
		public uint quests_completed { get; internal set; }
		public uint items_cooked { get; internal set; }
		public uint items_crafted { get; internal set; }
		public uint items_foraged { get; internal set; }
		public uint items_shipped { get; internal set; }
		public uint weeds_eliminated { get; internal set; }
		public uint monsters_killed { get; internal set; }
		public uint slimes_killed { get; internal set; }
	}

	internal class FarmerSkillLevelDTO
	{
		public int Minning { get; internal set; }
		public int Fighting { get; internal set; }
		public int Fishing { get; internal set; }
		public int Farming { get; internal set; }
		public int Foraging { get; internal set; }
		public int Luck { get; internal set; }
		public int Level { get; internal set; }
	}

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
				horse_name = _farmer.horseName.Value
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

		public FarmerSkillLevelDTO GetFamerLevel()
		{
			return new FarmerSkillLevelDTO
			{
				Minning = _farmer.MiningLevel,
				Fighting = _farmer.CombatLevel,
				Fishing = _farmer.FishingLevel,
				Farming = _farmer.FarmingLevel,
				Foraging = _farmer.ForagingLevel,
				Luck = _farmer.LuckLevel,
				Level = _farmer.Level
			};
		}
	}

	[Controllable]
	public class FarmerController : Controller
	{
		[JWT]
		[Get("/me")]
		public async Task GetCurrentFarmer([CurrentUser] JwtPayload payload)
		{
			var entity = new FarmerEntity(long.Parse(payload.Sub));
			await Json(entity.GetFarmerInformation());
		}
	}
}
