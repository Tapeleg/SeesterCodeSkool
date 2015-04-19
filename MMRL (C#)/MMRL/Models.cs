using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMRL
{
    //A "Model" is a "strategy." It holds some terribly named variables based on academic papers
    public class Model
    {
        private List<float> d; //"d" stores a list of values which hold the inferred value of a certain action
        private float lambda;  //lambda is a "learning speed" number. Basically, it tells the model how quickly to assume a conclusion
                               //less is more caution, and more actions until reaching a final decision, and more is Repbulican
        private int size;      //this stores the number of actions this model will keep track of

        public Model() { }     //empty constructor cuz "best practice"
        
        //Initialized a model based on the number of actions to choose upon, and the speed of convergence
        //lambda is set higher up on the "agent" level, so this number is just passed down
        public Model(int numActions, float speed) 
        { 
            size = numActions; 
            lambda = speed; 
            d = new List<float>();
            for (int i = 0; i < size; i++)
                d.Add(0);
        }

        //Realistically, I should have a boolean which is set once decisions are made, so this is a "don't copy this style of thinking" example
        //nothing to see here
        public void setValues(int index, float value) { d[index] = value; } //USE ONLY FOR INITIALIZATION

        //Looks back at the results of the last x actions (Window) to see how accurate this model has been based on the "real world"
        //This value is used by the Agent to select a model for the next action
        public float probability(ref Window wind, ref List<Action> actions) 
        {
            bool[] rein = new bool[wind.size()];
            int[] names = wind.get(ref rein);

            float prob = 1;

            for (int i = 0; i < rein.Length; i++) //iterate through window
                for (int j = 0; j < size; j++ )   //iterate through d
                    if (rein[i]) { prob *= d[names[i]]; }
                    else { prob *= (1 - d[names[i]]); }
                

            return prob;
        }

        //Updates the "d" (value) list with the result of the action taken
        public void update(int index, bool reinforced) 
        {
            int b;

            if (reinforced) { b = 1; }
            else { b = -1; }

            d[index] = (1 - lambda) * (d[index]) + (lambda * (b));

            if (d[index] < 0) { d[index] = 0; }

            //poor man's logging. Uncomment for more information
            /*Console.Write("[");
            for (int i = 0; i < size; i++)
                Console.Write(((int)(d[i] * 100)).ToString() + " ");
            Console.WriteLine("] ");*/

        }

        //Returns this Model's suggestion for the next action
        public int Best()
        {
            int best = 0;

            for (int i = 0; i < size; i++)
                if (d[i] > d[best])
                    best = i;

            return best;
        }
    }
}
