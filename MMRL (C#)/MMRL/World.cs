using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMRL
{
    //"World" class sets the stage, and causes the agent to act
    //This is more of an "interface" for several different potential worlds. Resuability and all that fun stuff
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

    //One implementation of a "World"
    class ManyActions : World 
    {
        ManyModels agent;
        List<Action> actions;
        int count = 0;
        bool go = true;
        float error = 0;

        //Creates actions, instantiates the right agent.
        protected override void init()
        {

            //sets up a list of actions, each with a percentage that it will provide a positive reinforcement
            //The job of the agent is to find the action with the highest reinforcement rate
            //The purpose of this whole project is to see if this "many-model" scheme speeds this process
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
                //If this line isn't there, then there isn't enough time for the agent to reset it's experiment
                //and we report the same expiriment multiple times
                System.Threading.Thread.Sleep(1200); 
            }
            else { go = false; Console.Write("Done!" + " " + (error)/50); Console.Read(); }
        }
    }

    //Simply an object which holds a percentage likelihood of positive reinforcement
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
