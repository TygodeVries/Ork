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
                if (!File.Exists("/secret/secrets.json"))
                {
                    Console.WriteLine("File does not exist!");
                    return null;
                }

                string text = File.ReadAllText("/secret/secrets.json");

                Console.WriteLine($"Read Secrets File! {text.Length}");
                instance = JsonSerializer.Deserialize<Secrets>(text);

                if (instance == null)
                {
                    Console.WriteLine("Could not load json object!");
                    return null;
                }

                Console.WriteLine($"Loaded Secrets!.");
                Console.WriteLine($"Initials: {instance.databaseUrl?.Substring(0, 5)}");
            }

            if (instance == null)
            {
                Console.WriteLine("Failed to load secrets.txt");
            }

            return instance!;
        }

        public string? databaseUrl { get; set; }
        public string? databaseKey { get; set; }
    }
}
