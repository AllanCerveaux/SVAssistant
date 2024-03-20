using System.IdentityModel.Tokens.Jwt;
using HttpServer.Framework;
using HttpServer.Framework.Decorator;
using Netcode;
using StardewValley;

namespace Compagnon.Controllers
{
	internal class FarmerDTO
	{
		public string name { get; set; }
		public int money { get; set; }
		public int total_money_earned { get; set; }
		public NetArray<int, NetInt> experiences { get; set; }
		public FamerStatsDTO statistics { get; set; }
	}

	internal class FamerStatsDTO
	{
		public uint days_played { get; set; }
		public uint times_unconscious { get; set; }
		public uint times_fished { get; set; }
		public uint average_bedtime { get; set; }
		public int mine_level { get; set; }
		public uint steps_taken { get; set; }
		public uint notes_found { get; set; }
		public uint pieces_of_trash_recycled { get; set; }
		public uint preserves_made { get; set; }
		public uint rabbit_wool_produced { get; set; }
		public uint dirt_hoed { get; set; }
		public uint stumps_chopped { get; set; }
		public uint crops_shipped { get; set; }
		public uint seeds_sown { get; set; }
		public uint truffles_found { get; set; }
		public uint cave_carrots_found { get; set; }
		public uint cooper_found { get; set; }
		public uint iron_found { get; set; }
		public uint gold_found { get; set; }
		public uint diamonds_found { get; set; }
		public uint iridium_found { get; set; }
		public uint other_precious_gems_found { get; set; }
		public uint prismatic_shards_found { get; set; }
		public uint stone_gathered { get; set; }
		public uint mystic_stones_crushed { get; set; }
		public uint rocks_crushed { get; set; }
		public uint beverage_made { get; set; }
		public uint cheese_made { get; set; }
		public uint goat_cheese_made { get; set; }
		public uint cow_milk_produced { get; set; }
		public uint goat_milk_produced { get; set; }
		public uint duck_eggs_layed { get; set; }
		public uint chicken_eggs_layed { get; set; }
		public uint sheep_wool_produced { get; set; }
		public uint fish_caught { get; set; }
		public uint geodes_cracked { get; set; }
		public uint gifts_given { get; set; }
		public uint good_friends { get; set; }
		public uint quests_completed { get; set; }
		public uint items_cooked { get; set; }
		public uint items_crafted { get; set; }
		public uint items_foraged { get; set; }
		public uint items_shipped { get; set; }
		public uint weeds_eliminated { get; set; }
		public uint monsters_killed { get; set; }
		public uint slimes_killed { get; set; }
	}

	internal class FarmerEntity
	{
		public static FarmerDTO GetFarmer(long id)
		{
			Farmer farmer = Game1.getFarmer(id);
			FamerStatsDTO statistics = GetFarmerStatistics(farmer.stats, farmer.deepestMineLevel);

			return new FarmerDTO
			{
				name = farmer.Name,
				money = farmer.Money,
				total_money_earned = (int)farmer.totalMoneyEarned,
				experiences = farmer.experiencePoints,
				statistics = statistics
			};
		}

		public static FamerStatsDTO GetFarmerStatistics(Stats stats, int mineLevel)
		{
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
	}

	[Controllable]
	public class FarmerController : Controller
	{
		[JWT]
		[Get("/me")]
		public async Task GetCurrentFarmer([CurrentUser] JwtPayload payload)
		{
			await Json(FarmerEntity.GetFarmer(long.Parse(payload.Sub)));
		}
	}
}
