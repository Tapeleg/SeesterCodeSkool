using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMRL
{
    //Agent is an "Actor" in the world, with a number of decisions (actions) it can make
    //Its goal is to pursue maximum reinforcement, and pinpoint which action has the highest likelihood of 
    //giving positive reinforcement
    public class Agent
    {
        protected List<Model> models; //A Model is a "strategy" that holds data on the actions, and suggests a course of action
        protected List<Action> actions;//List of potential actions an agent can take
        protected List<float> probs;//list of numbers which measure each model's likelihood of being accurate to the "real world"
        protected List<float> P;   //Equivalent of "d" variable in the model, but on agent level. Again, has poor name because of academic paper. 
                                   //lists the confidence of the agent that each action is the best one. algorithm ends when one P is greater than 90%
        protected float lambda = .01f; //<--ADJUST LEARNING SPEED PARAMETER HERE. This rules the "audacity" of the agent to make a fast decision. Higher lambda = higher rate of error
        protected int steps; //keeps track of how many steps it takes for the algorithm to converge
        protected Window window; //Window of last 5 actions and their results. Passed down to models to calculate probability.
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
                P.Add(1.0f / (float)actions.Count); //All actions start at the same confidence level. if 5 actions, start at 20% each
        }

        //Not currently used. Explores the potential idea that before making a decision on an action, an agent should get the lay of the land first. Try 'em all out once
        //This is called the "exploratory phase" in an AI algorithm, which may or may not happen before an Agent starts to pursue the optimal action
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

        //All the good stuff is in here. This is where the Agent:
            //a. chooses which model to act upon based on which one is the most realistic
            //b. acts based on that model's recommendation
            //c. updates all models with the result
        public void act()
        {
            bool done = false;
            int b = 0;

            for (int i = 0; i < actions.Count; i++)
                if (P[i] > .9) { b = i; done = true; break; } //checks for algorithm convergence

            //If the algorithm has converged, report findings and start experiment again
            if (done && !finished)
            {
                //Console.WriteLine("After " + steps + " the best action is " + b.ToString());
                world.restart(b, steps);
                finished = true;
                return;
            }

            //At this point, the algorithm hasn't yet converged, so we decide an action
            //First, check all probabilities that each model reflects the real world based on the last x actions/reinforcements that ACTUALLY happened
            int best = 0;
            probs = new List<float>();
            int ind = 0;

            for (int i = 0; i < models.Count; i++)
                probs.Add(models[i].probability(ref window, ref actions));

            for (int i = 0; i < probs.Count; i++)
                if (probs[i] > probs[ind])
                    ind = i;

            //populate best with all of those tied for best, then choose the random one.
            //in case that there are multiple candidate models for the "best choice"
            for (int i = 0; i < probs.Count; i++)
                if (probs[i] == probs[ind]) { best = i; break; }

            //Adds to the P value which corresponds to the action which the current best model suggests is the way to go
            //Also based on the learning speed lambda, which controls speed of algorithm convergence
            P[models[best].Best()] = P[models[best].Best()] * (1 - lambda) + lambda;
               
            //Now we use the Agent's metrics, updated by the best model, to randomly choose an action to act upon
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

           //Act on chosen action, and update all models on the result
           if (actions[toChoose].Reinforce(ref rand)) { rewarded = true; }
           else { rewarded = false; }

            for (int i = 0; i < models.Count; i++)
                models[i].update(toChoose, rewarded);

            //Update the window lookback, to calculate the models' accuracy next round
            window.shift(toChoose, rewarded);

            steps++;

            
        }

        protected virtual void assignModels()
        {   

        }
    }

    //One implementation of the Agent class
    public class ManyModels : Agent
    {
        protected int size;

        public ManyModels(ref List<Action> acts, int numModels, World thisworld) : base(ref acts, thisworld) { size = numModels; assignModels(); }

        //creates a number of models and evenly places them along the edges of an n-dimensional cube (SEE COMMENTS AT BOTTOM. WARNING: MATH)
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


            /*
             * So, what the hell is going on here?
             * 
             * What we're doing in this method is initializing the models as owned by this agent across a diverse range of possibilities.
             * 
             * Think about a potential action. Then think about it's likelihood for positive reinforcement. You can represent this as a line segment
             * from 0-1, with 0 representing 0% chance of reinforcement, and 1 being 100%. The actual probability is a point which is somewhere in the middle.
             * That is, the "truth" is a point somewhere on this line segment.
             * 
             * Now think about a grouping of 2 potential actions. The "truth" is a point which exists inside of a SQUARE. It has two coordinates: one for one action,
             * one for another.
             * 
             * Now think about a grouping of "n" potential actions. The "truth" is a point which exists inside of an n-dimensional hypercube.
             * 
             * Whoa.
             * 
             * So we want to find the "truth." A model, on its journey for truth, starts at one point and slowly moves towards the "truth" point. We have the luxury
             * in this expirement of having many models at once. So the goal of this method is to instantiate these models in such a way where they are in diverse
             * locations in a sort of "divide and conquer" mentality. (It would be lame to have them all start at the same place, no?)
             * 
             * Specifically, what's happening in this exact method is we're mapping the "starting" point of each model at some unique spot on the edges of this 
             * n-dimensional hypercube, so that they are all mostly equidistant from each other.
             * 
             * How do we do this?
             * 
             * Glad you asked!
             * 
             * First, a question: how many edges are in a 1-dimensional construct? One. It's just the line segment.
             * What about a square? How many edges? 4.
             * A cube? How many edges? 8
             * An n-dimensional hypercube? 2^n edges.
             * 
             * So we know that each dimension in our cube corresponds to an action. We also know how many models to assign on the edges of their collective cube.
             * 
             * Since every edge is 1 dimension, we can pretend to take all of the edges in this n-dimensional cube and line them up, end to end, to make a 
             * one-dimensional line segment with length n * 2^n (we just have n=1 because the size of the cube does not matter. This line segment becomes just 2^n)
             * 
             * Now that we've made this n-dimensional moster a 1-dimensional line segment, we just plop all the starting points on this segment equidistant from 
             * each other. Now, this line segment itself is... segmented. That is, we don't actually line up the edges end to end. We just pretend like we're doing
             * that. It's still a hypercube, and it's still n-dimensions. 
             * 
             * How the assignment really works is by utilizing the power of the binary number system. I'll ask a couple of question which were asked before:
             * How many binary numbers can you write with 1 digit? 2. 0 and 1
             * What about 2 digits? 4. 00, 01, 11, and 10
             * N digits? 2^n.
             * 
             * See where we're going with this?
             * 
             * These binary numbers represent coordinates for corners. Our line segment has end points 0 and 1, and if you think about a square in an x-y plane, 
             * you can say that the corners (0,0), (0,1), (1,0), and (1,1) define a 2-dimensional square.
             * 
             * Technically, these corners do exist on edges. And in our fake hypercube-turned-line-segment, they represent landmarks, and as boundaries between edges.
             * 
             * SO. In our square, we have four corners, as discussed. If we take these coordinates, and imagine them as binary numbers, we can order them:
             * 
             * 00
             * 01
             * 10
             * 11
             * 
             * aka 0, 1, 2, 3 in binary. So our max value is 3 here. If I wanted to assign five points onto this, they'd all need to be 4/5 apart, starting at 0.
             * 
             * 0 = 0, 0
             * 0.8 = 0, .8
             * 1.6 = .6, 0
             * 2.4 =  .4, 1
             * 3 = 1, 1
             * 
             * The above example would represent a world whose agent holds 5 models, and has 2 actions to choose from. 
             * 
             * As you would expect, I ran into a plateau of convergence speed, since I wasn't filling in the area of this cube, only the edges. That is,
             * filling up the edges of a square with 10,000,000 models vs 10,000 models doesn't make much of a difference, even though the number is 1000x greater.
             * 
             * However, I've always been curious as to how one would put x points in the area of a y-dimensional cube all mostly equidistant from each other.
             * 
             * Something to geek out to on a rainy day perhaps.
             */
        }
    }

#region Utility Classes

    //This guy just holds a window of the last 5 actions taken, with the reinforcement of each
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
