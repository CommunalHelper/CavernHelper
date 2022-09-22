﻿using System.Collections;
using Monocle;

namespace Celeste.Mod.CavernHelper
{
    public class DialogCutscene : CutsceneEntity
    {
        private Player player;
        private string dialogEntry;
        private bool endLevel;

        public DialogCutscene(string dialogID, Player playerEnt, bool _endLevel) : base(true, false)
        {
            dialogEntry = dialogID;
            player = playerEnt;
            endLevel = _endLevel;
        }

        public override void OnBegin(Level level)
        {
            base.Add(new Coroutine(this.Cutscene(level), true));
        }

        private IEnumerator Cutscene(Level level)
        {
            this.player.StateMachine.State = 11;
            this.player.StateMachine.Locked = true;
            this.player.ForceCameraUpdate = true;
            yield return Textbox.Say(dialogEntry, null);
            this.EndCutscene(level, true);
            yield break;
        }

        public override void OnEnd(Level level)
        {
            this.player.StateMachine.Locked = false;
            this.player.StateMachine.State = 0;
            this.player.ForceCameraUpdate = false;
            bool wasSkipped = this.WasSkipped;
            if (wasSkipped)
            {
                level.Camera.Position = this.player.CameraTarget;
            }
            if (endLevel)
            {
                (base.Scene as Level).CompleteArea(true, false);
                player.StateMachine.State = Player.StDummy;
                base.RemoveSelf();
            }
        }
    }
}