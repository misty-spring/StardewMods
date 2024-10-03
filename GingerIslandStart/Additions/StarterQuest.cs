using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.Quests;

namespace GingerIslandStart.Additions;

///for the starter quest, see <see cref="StardewValley.Quests.SocializeQuest"/>
public class StarterQuest : Quest
{
    [XmlElement("total")]
    public readonly NetInt total = new NetInt();
    [XmlElement("objective")]
    public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

    public StarterQuest() => this.questType.Value = 5;

    protected override void initNetFields()
    {
      base.initNetFields();
      this.NetFields.AddField((INetSerializable) this.total, "total").AddField((INetSerializable) this.objective, "objective");
    }

    public void loadQuestInfo()
    {
      if (this.total.Value > 0)
        return;
      this.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:StartInGI_StarterQuest_Title");
      this.total.Value = ModEntry.Config.Difficulty switch
      {
        "easy" => 15,
        "hard" => 60,
        _ => 30
      };
      this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:StartInGI_StarterQuest_Progress", new object[2]
      {
        (object) (this.total.Value - Game1.player.stats.StoneGathered),
        (object) this.total.Value
      });
    }

    public override void reloadDescription()
    {
      if (this._questDescription == "")
        this.loadQuestInfo();
      var limit = ModEntry.Config.Difficulty switch
      {
        "easy" => 15,
        "hard" => 60,
        _ => 30
      };
      
      //if limit was reached
      if (this.total.Value <= limit)
        return;
      
      this.questDescription = Game1.content.LoadString("Strings\\StringsFromCSFiles:StartInGI_StarterQuest_Description");;
    }

    public override void reloadObjective()
    {
      this.loadQuestInfo();
      if (this.objective.Value == null && this.whoToGreet.Count > 0)
        this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13802", new object[2]
        {
          (object) (this.total.Value - this.whoToGreet.Count),
          (object) this.total.Value
        });
      if (this.objective.Value == null)
        return;
      this.currentObjective = this.objective.Value.loadDescriptionElement();
    }

    public override bool checkIfComplete(
      NPC npc = null,
      int number1 = -1,
      int number2 = -1,
      Item item = null,
      string monsterName = null,
      bool probe = false)
    {
      this.loadQuestInfo();
      
      var limit = ModEntry.Config.Difficulty switch
      {
        "easy" => 15,
        "hard" => 60,
        _ => 30
      };
      
      if (!probe)
      {
        Game1.dayTimeMoneyBox.moneyDial.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(387, 497, 3, 8), 800f, 1, 0, Game1.dayTimeMoneyBox.position + new Vector2(228f, 244f), false, false, 1f, 0.01f,  Color.White, 4f, 0.3f, 0.0f, 0.0f)
        {
          scaleChangeChange = -0.012f
        });
        Game1.dayTimeMoneyBox.pingQuest((Quest) this);
      }
      if (limit <= Game1.player.stats.StoneGathered && !this.completed.Value)
      {
        if (!probe)
        {
          Game1.player.Money += 500;
          this.questComplete();
        }
        return true;
      }
      
      if (!probe)
        this.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:StartInGI_StarterQuest_Progress", new object[2]
        {
          (object) (this.total.Value - Game1.player.stats.StoneGathered),
          (object) this.total.Value
        });
      return false;
    }
  }
}