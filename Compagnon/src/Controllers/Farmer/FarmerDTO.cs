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
		public FamerStatsDTO statistics { get; internal set; }
		public List<FarmerSkillDTO> skills { get; internal set; }
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

	public class FarmerSkillDTO
	{
		public int level { get; internal set; }
		public string name { get; internal set; }
		public List<ProfessionDTO> professions { get; internal set; }
	}
}
