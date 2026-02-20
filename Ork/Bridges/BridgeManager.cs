using Ork.Users;

namespace Ork.Bridges
{
    public class BridgeManager
    {
        /// <summary>
        /// The size (amount of characters) of a code
        /// </summary>
        private const int CODE_SIZE = 5;

        /// <summary>
        /// List of tokens that the code generator can use, excluding look-a-like characters
        /// </summary>
        private const string CODE_CHARACTERS = "ABCDEFGHIJKLMNOPRSTUW";

        private static List<string> activeCodes = new List<string>();
        private static List<Bridge> bridges = new List<Bridge>();

        private static string GetUniqueCode()
        {
            Random rng = new Random();
            string code = "";
            for (int i = 0; i < CODE_SIZE; i++)
            {
                code += CODE_CHARACTERS[rng.Next(CODE_CHARACTERS.Length)];
            }

            // Could be done better
            bool codeAlreadyExists = activeCodes.Contains(code);
            if (codeAlreadyExists)
                return GetUniqueCode();

            activeCodes.Add(code);

            return code;
        }

        public static Bridge CreateBridge()
        {
            string code = GetUniqueCode();

            Bridge bridge = new Bridge(code);
            bridges.Add(bridge);

            Console.WriteLine($"Created bridge with code {code}");
            return bridge;
        }

        public static Bridge? GetBridge(string code)
        {
            return bridges.SingleOrDefault(bridge => bridge!.Code == code.ToUpper(), null);
        }

        public static Bridge? GetBridge(User user)
        {
            foreach (Bridge bridge in bridges)
            {
                if (bridge.Game == user || bridge.Phone == user)
                    return bridge;
            }

            return null;
        }

        public static void Remove(Bridge bridge)
        {
            bridges.Remove(bridge);
        }
    }
}
