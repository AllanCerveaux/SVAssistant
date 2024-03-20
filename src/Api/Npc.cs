using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using SVAssistant.Decorator;
using SVAssistant.Http.Routes;

namespace SVAssistant.Api
{
	public class NpcDTO
	{
		public int id;
		public string portrait { set; get; }
		public int gender { set; get; }
		public int age { set; get; }
		public int birthday_day { set; get; }
		public string birthday_season { set; get; }
		public bool can_socialize { set; get; }

		public string name { set; get; }
	}

	public class NpcEntity
	{
		public static IEnumerable<NPC> EnumAll()
		{
			return Game1.locations.SelectMany(location => location.characters.OfType<NPC>());
		}

		public static IEnumerable<NPC> EnumVillagers()
		{
			return EnumAll().Where(npc => npc.isVillager());
		}

		public static List<NpcDTO> Villagers()
		{
			return EnumVillagers()
				.Select(villager =>
				{
					return new NpcDTO
					{
						id = villager.id,
						name = villager.Name,
						portrait = villager.Portrait.ToString(),
						gender = villager.Gender,
						age = villager.Age,
						birthday_day = villager.Birthday_Day,
						birthday_season = villager.Birthday_Season,
						can_socialize = villager.CanSocialize,
					};
				})
				.ToList();
		}

		// public static Dictionary<string, List<int>> GetNpcLikes()
		// {
		// 	var npcGiftTastes = Game1.content.Load<Dictionary<string, string>>("Data/NPCGiftTastes");

		// }
	}

	public class NpcController : Controller
	{
		public NpcController(
			IHttpResponseService response,
			IHttpRequestService request,
			IRouteHandler route
		)
			: base(response, request, route) { }

		[Get("/villagers")]
		public async Task Villagers()
		{
			var npcGiftTastes = Game1.content.Load<Dictionary<string, string>>(
				"Data/NPCGiftTastes"
			);
			foreach (var entry in npcGiftTastes)
			{
				var tastes = entry.Value.Split('/');
				ModEntry.Logger.Log($"{tastes}", StardewModdingAPI.LogLevel.Info);
			}
			await Json(NpcEntity.Villagers());
		}
	}
}
