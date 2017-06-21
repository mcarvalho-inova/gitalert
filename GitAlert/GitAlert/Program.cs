namespace GitAlert
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            GitAlert.ChamadasGit Chamadas = new GitAlert.ChamadasGit();

            Chamadas.CarregarCommits();
        }
    }
}