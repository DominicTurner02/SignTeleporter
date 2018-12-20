using Newtonsoft.Json.Linq;
using Rocket.API;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SignTeleporter
{
    public class SignTeleporter : RocketPlugin<ConfigurationSignTeleporter>
    {
        public static SignTeleporter Instance { get; private set; }
        public List<string> AllowedIDs = new List<string>();

        protected override void Load()
        {
            base.Load();
            Instance = this;
            Logger.LogWarning("\nLoading SignTeleporter, made by Mr.Kwabs...");
            if (!File.Exists("Plugins/SignTeleporter/AllowedIDs.json"))
            {
                Logger.LogError("\nAllowedIDs.json does not exist, creating it now...");
                FirstLoad();
                Logger.LogWarning("Successfully created AllowedIDs.json.");
            }
            SendList();
            Logger.LogWarning($"\nFound AllowedIDs.json, read {AllowedIDs.Count} IDs.");
            Logger.LogWarning("\nID List: \n");
            foreach (string ID in AllowedIDs)
            {
                Logger.LogWarning($"{ID}");
            }
            Logger.LogWarning($"\nMaximum Distance from Sign to Teleport: {Instance.Configuration.Instance.MaxDistance}");
            UnturnedPlayerEvents.OnPlayerUpdateGesture += OnUpdatedGesture;
            Logger.LogWarning("\nSuccessfully loaded SignTeleporter, made by Mr.Kwabs!");
        }

        protected override void Unload()
        {
            AllowedIDs.Clear();
            UnturnedPlayerEvents.OnPlayerUpdateGesture += OnUpdatedGesture;
            Instance = null;
            base.Unload();
        }

        private void FirstLoad()
        {
            JArray IDArray = new JArray(
                new JArray("765xxxx", "765xxxx"));
            File.WriteAllText("Plugins/SignTeleporter/AllowedIDs.json", IDArray.ToString());
        }

        private void SendList()
        {
            JArray IDArray = JArray.Parse(File.ReadAllText("Plugins/SignTeleporter/AllowedIDs.json"));
            foreach (string ID in IDArray)
            {
                AllowedIDs.Add(ID);
            }
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
                    if (AllowTeleport(RayCast.GetComponent<InteractableSign>()))
                    {
                        ManageSign(uCaller, RayCast.GetComponent<InteractableSign>());
                        return;
                    }
                    Logger.LogError("The Owner of the Sign is not in AllowedIDs.json!");       
                }
            }
            return;
        }


        private bool AllowTeleport(InteractableSign Sign)
        {
            if (AllowedIDs.Contains(Sign.owner.ToString()))
            {
                return true;
            }
            return false;
        }

        private void ManageSign(UnturnedPlayer uPlayer, InteractableSign Sign)
        {

            if (Sign.text == null || Sign.text == "" || !Sign.text.Contains("~") || !uPlayer.HasPermission("signteleporter.teleport")) { return; }
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
