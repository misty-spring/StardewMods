﻿using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmVisitors
{
    internal class EntryQuestion : DialogueBox
    {
        private readonly List<Action> ResponseActions;

        /// <summary>
        /// Creates a new TextboxAction.
        /// </summary>
        /// <param name="dialogue"> The dialogue to display.</param>
        /// <param name="responses">Responses.</param>
        /// <param name="Actions">Actions associated with said responses.</param>
        internal EntryQuestion(string dialogue, List<Response> responses, List<Action> Actions) : base(dialogue, responses)
        {
            this.ResponseActions = Actions;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            int responseIndex = this.selectedResponse;
            base.receiveLeftClick(x, y, playSound);

            if (base.safetyTimer <= 0 && responseIndex > -1 && responseIndex < this.ResponseActions.Count && this.ResponseActions[responseIndex] != null)
            {
                this.ResponseActions[responseIndex]();
            }
        }
    }
}
