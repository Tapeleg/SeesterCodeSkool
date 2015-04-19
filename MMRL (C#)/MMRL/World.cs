using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMRL
{
    public class World
    {
        protected virtual void init()
        {

        }

        public World() { init(); }
        

        public virtual void run()
        {

        }

        public virtual void restart(int selection, int steps)
        {

        }
    }

    class ManyActions : World 
    {
        //FourModels agent;
        ManyModels agent;
        List<Action> actions;
        int count = 0;
        bool go = true;
        float error = 0;

        protected override void init()
        {
            actions = new List<Action>();

            actions.Add(new Action(80));
            actions.Add(new Action(70));
            actions.Add(new Action(60));
            actions.Add(new Action(20));
            actions.Add(new Action(55));
            actions.Add(new Action(50));
            actions.Add(new Action(10));
            actions.Add(new Action(25));
            actions.Add(new Action(75));
            actions.Add(new Action(33));

            //agent = new FourModels(ref actions);
            //agent = new SingleAgent(ref actions);
            agent = new ManyModels(ref actions, 1, this);
            
        }

        public override void run()
        {
           //agent.Explore();
            Console.Read();

           while(go)
           {
               agent.act();
           }
        }

        public override void restart(int selection, int steps)
        {
            string w = " ";
            

            if (selection != 0) { w = "!"; error++; }

            count++;
            if (count <= 50)
            {
                init();
                Console.WriteLine(w + ":" + steps.ToString());

                //gives time for the environment to reset before continuing
                System.Threading.Thread.Sleep(1200); 
            }
            else { go = false; Console.Write("Done!" + " " + (error)/50); Console.Read(); }
        }
    }

    public class Action
    {

        private int chance; //NAME MUST BE ITS INDEX IN LIST

        public Action(int rewardchance) { chance = rewardchance; }

        public bool Reinforce(ref Random rand)
        {
            if (rand.Next(101) < chance) return true;

            return false;
        }

    }
}
