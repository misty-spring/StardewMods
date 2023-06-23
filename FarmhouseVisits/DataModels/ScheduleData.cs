using System.Collections.Generic;

namespace FarmVisitors
{
    //the info for SCHEDULED npcs
    internal class ScheduleData
    {
        public int From { get; set; }
        public int To { get; set; }

        public string EntryBubble { get; set; }
        public string EntryQuestion { get; set; }

        public string EntryDialogue { get; set; }
        public string ExitDialogue { get; set; }

        public List<string> Dialogues { get; set; }

        public ExtraBehavior Force {get;set;} = new ExtraBehavior();

        public ScheduleData()
        {
        }

        public ScheduleData(ScheduleData sd)
        {
            From = sd.From;
            To = sd.To;

            EntryBubble = sd.EntryBubble;
            EntryQuestion = sd.EntryQuestion;

            EntryDialogue = sd.EntryDialogue;
            ExitDialogue = sd.ExitDialogue;

            Dialogues = sd.Dialogues;

            Force = sd.Force;
        }
    }
    //data temporarily stored about visiting npc

    internal class ExtraBehavior
    {
        public bool Enable {get;set;}
        public string Mail {get;set;}

        public ExtraBehavior()
        {
        }

        public ExtraBehavior(ExtraBehavior b)
        {
            Enable = b.Enable;
            Mail = b.Mail;
        }

    }
}