namespace CanBusDisplay
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 2 && args[2] == "debug")
            {
                new Controller(Config.Load("config.json"), new FakeDataSource("output.txt")).Start();
            }
            else
            {
                new Controller(Config.Load("config.json"), new DataSource(args[0], int.Parse(args[1]))).Start();
            }
        }
    }
}
