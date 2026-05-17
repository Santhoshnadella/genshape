using System;
using PicoGK;
using AutomotiveEngineering;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Initializing GenShape Computational Engineering Engine...");
        
        GenShapeServer server = new GenShapeServer();
        server.Start();

        Console.WriteLine("\n[Ready] Press Ctrl+C or Enter to shut down the server.");
        Console.ReadLine();

        server.Stop();
    }
}
