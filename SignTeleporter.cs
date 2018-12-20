using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SignTeleporter
{
    public class SignTeleporter : RocketPlugin<ConfigurationSignTeleporter>
    {
        public static SignTeleporter Instance { get; private set; }

        protected override void Load()
        {
            base.Load();
            Instance = this;
            Logger.LogWarning("\n Loading SignTeleporter, made by Mr.Kwabs...");
            UnturnedPlayerEvents.OnPlayerUpdateGesture += OnUpdatedGesture;
            Logger.LogWarning("\n Successfully loaded SignTeleporter, made by Mr.Kwabs!");
        }

        protected override void Unload()
        {
            UnturnedPlayerEvents.OnPlayerUpdateGesture += OnUpdatedGesture;
            Instance = null;
            base.Unload();
        }

        private void OnUpdatedGesture(UnturnedPlayer uCaller, UnturnedPlayerEvents.PlayerGesture Gesture)
        {
            if (Gesture == UnturnedPlayerEvents.PlayerGesture.PunchLeft)
            {
                Transform RayCast = GetRaycast(uCaller);

                if (RayCast == null)
                {
                    return;
                }

                if (RayCast.GetComponent<InteractableSign>() != null)
                {
                    ManageSign(uCaller, RayCast.GetComponent<InteractableSign>());
                }
            }
            return;
        }


        private void ManageSign(UnturnedPlayer uPlayer, InteractableSign Sign)
        {

            if (Sign.text == null || Sign.text == "" || !Sign.text.Contains("~")) { return; }
            int[] Coordinates;
            try
            {
                Coordinates = Array.ConvertAll(Sign.text.Split('~', '~')[1].Split(','), s => int.Parse(s));
            } catch (Exception ex)
            {
                UnturnedChat.Say(uPlayer, "There has been an error teleporting from sign!", Color.red);
                Logger.LogWarning($"There has been an Error, contact Mr.Kwabs#9751 or ChingoonPvP@gmail.com with the following:\n[Recieving Coordinates]");
                Logger.Log($"{ex}", ConsoleColor.DarkYellow);
                return;
            }

            try
            {
                uPlayer.Teleport(new Vector3(Coordinates[0], Coordinates[1], Coordinates[2]), uPlayer.Player.look.rot);
            } catch (Exception ex)
            {
                UnturnedChat.Say(uPlayer, "There has been an error teleporting from sign!", Color.red);
                Logger.LogWarning($"There has been an Error, contact Mr.Kwabs#9751 or ChingoonPvP@gmail.com with the following:\n[Attempting Teleport]");
                Logger.Log($"{ex}", ConsoleColor.DarkYellow);
                return;
            }
            UnturnedChat.Say(uPlayer, "Successfully teleported from Sign!", Color.yellow);
            Logger.LogWarning($"{uPlayer.DisplayName} has teleported to {new Vector3(Coordinates[0], Coordinates[1], Coordinates[2])} from a sign!");
            return;            
        }


        private Transform GetRaycast(UnturnedPlayer uPlayer)
        {
            if (Physics.Raycast(uPlayer.Player.look.aim.position, uPlayer.Player.look.aim.forward, out RaycastHit RayHit, Instance.Configuration.Instance.MaxDistance, RayMasks.BARRICADE_INTERACT))
            {
                return RayHit.transform;
            }
            return null;
        }
    }
}
