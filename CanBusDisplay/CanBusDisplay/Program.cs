namespace CanBusDisplay
{
    class Program
    {
        static void Main(string[] args)
        {
            new Controller(Config.Load("config.json"), new DataSource(args[0], int.Parse(args[1]))).Start();
        }
    }
}
