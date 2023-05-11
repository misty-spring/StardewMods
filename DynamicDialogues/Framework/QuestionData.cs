using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using StardewModdingAPI;
using StardewValley;

namespace DynamicDialogues.Framework;

public class QuestionData : Dialogue
{
  //private string _questToAdd;
  //private bool _isLastDialogueInteractive = false;
  
  private List<NPCDialogueResponse> _playerResponses = new();
  private List<string> _quickResponses = new();
  private List<string> _missionList = new();
  
  public QuestionData(NPC speaker, string dialogueText, string translationKey = null) : base(speaker, translationKey, dialogueText)
    {
        //this.quickResponse = true;
        this.speaker = speaker;
        
        /*if(!string.IsNullOrWhiteSpace(QuestID))
          this.onFinish = new Action(AddQuest);
        
        this._questToAdd = QuestID;*/
        
        try
        {
            this.parseDialogueString(dialogueText);
            //checkForSpecialDialogueAttributes();
        }
        catch (Exception ex)
        {
            //IGameLogger log = Game1.log;
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 3);
            interpolatedStringHandler.AppendLiteral("Failed parsing dialogue string for NPC ");
            interpolatedStringHandler.AppendFormatted(speaker?.Name);
            interpolatedStringHandler.AppendLiteral(" (key: ");
            interpolatedStringHandler.AppendFormatted(translationKey);
            interpolatedStringHandler.AppendLiteral(", text: ");
            interpolatedStringHandler.AppendFormatted(dialogueText);
            interpolatedStringHandler.AppendLiteral(").");
            string stringAndClear = interpolatedStringHandler.ToStringAndClear();
            Exception exception = ex;
            ModEntry.Mon.Log(stringAndClear,LogLevel.Error);
            this.parseDialogueString("...");
        }
    }

  protected override void parseDialogueString(string masterString)
    {
      var source = masterString.Split('#');
      if (source[0] == "$qna")
      {
        var questions = source[1].Split('_');
        var answers = source[2].Split('_');
        var missions = source[3].Split('_');

        //this._isLastDialogueInteractive = true;
        
        this._quickResponses ??= new List<string>();
        this._playerResponses ??= new List<NPCDialogueResponse>();
        this._missionList ??= new List<string>();
        
        foreach (var count in questions)
        {
          var index = GetIndex(questions,count);
          
          this._playerResponses.Add(new NPCDialogueResponse(null, -1, "quickResponse" + index.ToString(), Game1.parseText(count)));
          this._quickResponses.Add(answers[index]);
          this._missionList.Add(missions[index]);
        }
      }
    }
  public override bool chooseResponse(Response response)
  {
      for (var index = 0; index < this._playerResponses.Count; ++index)
      {
        if (this._playerResponses[index].responseKey != null && response.responseKey != null && this._playerResponses[index].responseKey.Equals(response.responseKey))
        {
          //get dialogue
          this.speaker.setNewDialogue(new Dialogue(this.speaker, (string) null, this._quickResponses[index]));
          Game1.drawDialogue(this.speaker);
          
          //face farmer
          this.speaker.faceTowardFarmerForPeriod(4000, 3, false, this.farmer);
          
          //if mission, add
          if (_missionList[index] != "none")
          {
            Game1.player.addQuest(_missionList[index]);
          }
          
          return true;
        }
      }
      return false;
    }
    
    /*
    private void AddQuest()
    {
      Game1.player.addQuest(this._questToAdd);
    }*/
  private static int GetIndex(string[] questions, string which)
    {
      var count = 0;
      foreach (var text in questions)
      {
        if (text != which)
        {
          count++;
        }
        else
        {
          return count;
        }
      }
      throw new KeyNotFoundException();
    }
}