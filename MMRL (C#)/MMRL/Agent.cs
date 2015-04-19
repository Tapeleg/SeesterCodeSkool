using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMRL
{
    public class Agent
    {
        protected List<Model> models;
        protected List<Action> actions;
        protected List<float> probs;
        protected List<float> P;
        protected float lambda = .01f; //<--ADJUST LEARNING SPEED PARAMETER HERE
        protected int steps;
        protected Window window;
        protected World world;
        protected Random rand;
        protected bool finished = false;

        public Agent(ref List<Action> acts, World thisworld) 
        {
            actions = acts; 
            models = new List<Model>();
            window = new Window();
            rand = new Random();
            steps = 0;
            world = thisworld;

            P = new List<float>();
            for (int i = 0; i < actions.Count; i++)
                P.Add(1.0f / (float)actions.Count);
        }

        public void Explore()
        {
            bool rewarded = false;

            for (int i = 0; i < actions.Count; i++)
                for (int j = 0; j < 3; j++)
                {
                    if (actions[i].Reinforce(ref rand)) { rewarded = true; }
                    else { rewarded = false; }

                    for (int k = 0; k < models.Count; k++)
                        models[k].update(i, rewarded);

                    window.shift(i, rewarded);

                    steps++;
                }

            
        }

        public void act()
        {
            bool done = false;
            int b = 0;

            for (int i = 0; i < actions.Count; i++)
                if (P[i] > .9) { b = i; done = true; break; }

            if (done && !finished)
            {
                //Console.WriteLine("After " + steps + " the best action is " + b.ToString());
                world.restart(b, steps);
                finished = true;
                return;
            }

            int best = 0;
            probs = new List<float>();
            int ind = 0;

            for (int i = 0; i < models.Count; i++)
                probs.Add(models[i].probability(ref window, ref actions));

            for (int i = 0; i < probs.Count; i++)
                if (probs[i] > probs[ind])
                    ind = i;

            //populate best with all of those tied for best, then choose the random one.
            for (int i = 0; i < probs.Count; i++)
                if (probs[i] == probs[ind]) { best = i; break; }

            P[models[best].Best()] = P[models[best].Best()] * (1 - lambda) + lambda;
            

            int toChoose = 0;
            float total = 0;
            bool rewarded;

            for (int i = 0; i < actions.Count; i++)
                total += P[i];

            float choice = (float)rand.NextDouble() * total;

            for (int i = 0; i < actions.Count; i++)
            {
                choice -= P[i];
                if (choice < 0) { toChoose = i; break; }
            }
            /*Console.Write("Choosing action " + toChoose.ToString() + ": ");

            Console.Write("[ ");
            for (int i = 0; i < P.Count; i++ )
            {
                Console.Write(" " + P[i].ToString() + " ");
            }
            Console.WriteLine("]");*/

           //use P vector to choose action
           if (actions[toChoose].Reinforce(ref rand)) { rewarded = true; }
           else { rewarded = false; }

            for (int i = 0; i < models.Count; i++)
                models[i].update(toChoose, rewarded);

            window.shift(toChoose, rewarded);

            steps++;

            
        }

        protected virtual void assignModels()
        {   

        }
    }

    public class ManyModels : Agent
    {
        protected int size;

        public ManyModels(ref List<Action> acts, int numModels, World thisworld) : base(ref acts, thisworld) { size = numModels; assignModels(); }

        //creates a number of models and evenly places them along the edges of an n-dimensional cube
        protected override void assignModels()
        {

            int edges;
            //Construct segmented line, populate list of pseudopositions
            float space = actions.Count;

            for (int i = 0; i < actions.Count - 1; i++)
                space *= 2;

            edges = (int)space;

            space /= size;

            float[] line = new float[size]; //contains segmented line to compare against
            for (int i = 0; i < size; i++)
                line[i] = space + (i * space);

            int bigbit = 1; //will contain 2^n
            
            
            int temp = 1;
            //assign initial model values
            for (int i = 0; i < size; i++)
            {
                models.Add(new Model(actions.Count, lambda));
                temp = (int)line[i];

                

                for (int k = 4; k < actions.Count; k++) //set up big bit
                    bigbit *= 2;

                //convert temp to binary, give values to model
                for(int j = 0; j < actions.Count; j++)
                {
                    if (temp / bigbit >= 1) { temp -= bigbit; models[i].setValues(j, 1); } //flip bit
                    else { bigbit /= 2; } //update parameters

                    //check for termination
                    if (temp == 0 && j < actions.Count - 1) { models[i].setValues(j + 1, line[i] - (int)line[i]); break; }
                }
                  
                
            }



        }
    }

#region Utility Classes

    public class Window
    {
        private int windowSize = 5;
        private List<int> actions;
        private List<bool> reinforcement;

        public Window() 
        {
            actions = new List<int>();
            reinforcement = new List<bool>();
        }

        public void shift(int act, bool rein) 
        {
            actions.Add(act);
            reinforcement.Add(rein);

            if(actions.Count > windowSize)
            {
                actions.RemoveAt(0);
                reinforcement.RemoveAt(0);
            }
        }

        public int[] get(ref bool[] result)
        {
            result = reinforcement.ToArray();
            return actions.ToArray();
        }

        public int size() { return actions.Count; }
    }

#endregion
}
