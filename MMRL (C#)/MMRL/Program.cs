using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMRL
{
    class Program
    {
        static void Main(string[] args)
        {
            //ManyActions is a subclass of "World"
            //"World" sets the stage, creates the events, and creates the players
            //Main only serves to "bootstrap" the world, which takes care of everything else
            ManyActions w = new ManyActions();

            w.run(); //LET THERE BE LIGHT

            Console.ReadLine();
        }
    }
}
