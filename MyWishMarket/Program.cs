namespace MyWishMarket
{
    class Program
    {
        static void Main(string[] args)
        {
            BotManager botManager = new BotManager();
            botManager.Start();
            Console.ReadLine();
        }
    }
}