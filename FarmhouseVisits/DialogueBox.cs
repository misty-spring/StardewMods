﻿using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;

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
        internal EntryQuestion(string dialogue, Response[] responses, List<Action> Actions) : base(dialogue, responses)
        {
            ResponseActions = Actions;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            var responseIndex = selectedResponse;
            base.receiveLeftClick(x, y, playSound);

            if (safetyTimer <= 0 && responseIndex > -1 && responseIndex < ResponseActions.Count && ResponseActions[responseIndex] != null)
            {
                ResponseActions[responseIndex]();
            }
        }
    }
}
