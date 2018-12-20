using Rocket.API;

namespace SignTeleporter
{
    public class ConfigurationSignTeleporter : IRocketPluginConfiguration
    {
        public int MaxDistance;

        public void LoadDefaults()
        {
            MaxDistance = 5;
        }

    }
}

