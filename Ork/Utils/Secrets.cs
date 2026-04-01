using System.Text.Json;

namespace Ork.Utils
{
    public class Secrets
    {
        private static Secrets? instance;
        public static Secrets GetSecrets()
        {
            if (instance == null)
            {
                string text = File.ReadAllText("/secret/secrets.json");
                instance = JsonSerializer.Deserialize<Secrets>(text);
                Console.WriteLine($"Loaded Secrets! Initials: {instance.databaseUrl.Substring(0, 5)}.");
            }

            if (instance == null)
            {
                Console.WriteLine("Failed to load secrets.txt");
            }

            return instance!;
        }

        public string? databaseUrl;
        public string? databaseKey;
    }
}
