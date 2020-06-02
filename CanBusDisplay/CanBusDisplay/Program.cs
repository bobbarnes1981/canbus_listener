namespace CanBusDisplay
{
    class Program
    {
        static void Main(string[] args)
        {
            new Controller(new DataSource(args[0], int.Parse(args[1]))).Start();
        }
    }
}
