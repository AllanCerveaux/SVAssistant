using StardewValley;
using StardewValley.Menus;

namespace Compagnon.Controllers
{
	public class FarmerSkillEntity
	{
		private Farmer _farmer;
		private int _id;

		public FarmerSkillEntity(Farmer farmer, int id)
		{
			_farmer = farmer;
			_id = id;
		}

		public int getSkillLevel()
		{
			return _farmer.GetSkillLevel(_id);
		}

		public string getSkillName()
		{
			return Farmer.getSkillNameFromIndex(_id);
		}

		public List<ProfessionDTO> getProfessions()
		{
			List<ProfessionDTO> professions = new List<ProfessionDTO>();
			List<int> whichProfessions = getProfessionBySkillId(_id);

			foreach (var whichProfession in whichProfessions)
			{
				string professionTitle = "";
				string professionBlurb = "";
				parseProfession(
					ref professionBlurb,
					ref professionTitle,
					LevelUpMenu.getProfessionDescription(whichProfession)
				);
				professions.Add(
					new ProfessionDTO
					{
						title = professionTitle,
						description = professionBlurb,
						unlock = GameStateQuery.CheckConditions(
							$"PLAYER_HAS_PROFESSION Current {whichProfession}"
						)
					}
				);
			}

			return professions;
		}

		private void parseProfession(
			ref string professionBlurb,
			ref string professionTitle,
			List<string> professionDescription
		)
		{
			if (professionDescription.Count <= 0)
			{
				return;
			}

			professionTitle = professionDescription[0];
			for (int i = 1; i < professionDescription.Count; i++)
			{
				professionBlurb += professionDescription[i];
			}
		}

		public List<int> getProfessionBySkillId(int id)
		{
			if (id < 0 || id > 5)
			{
				throw new Exception("The skill id must be between 0 and 4");
			}
			switch (id)
			{
				case 0:
					return new List<int> { 0, 1, 2, 3, 4, 5 };
				case 1:
					return new List<int> { 6, 7, 8, 9, 10, 11 };
				case 2:
					return new List<int> { 12, 13, 14, 15, 16, 17 };
				case 3:
					return new List<int> { 18, 19, 20, 22, 23 };
				case 4:
					return new List<int> { 24, 25, 26, 27, 28, 29 };
			}

			return new List<int>();
		}

		public FarmerSkillDTO Get()
		{
			return new FarmerSkillDTO
			{
				level = getSkillLevel(),
				name = getSkillName(),
				professions = getProfessions()
			};
		}
	}
}
