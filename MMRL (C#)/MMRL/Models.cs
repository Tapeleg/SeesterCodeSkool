using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMRL
{
    public class Model
    {
        private List<float> d;
        private float lambda;
        private int size;

        public Model() { }
        public Model(int numActions, float speed) 
        { 
            size = numActions; 
            lambda = speed; 
            d = new List<float>();
            for (int i = 0; i < size; i++)
                d.Add(0);
        }

        public void setValues(int index, float value) { d[index] = value; } //USE ONLY FOR INITIALIZATION

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

        public void update(int index, bool reinforced) 
        {
            int b;

            if (reinforced) { b = 1; }
            else { b = -1; }

            d[index] = (1 - lambda) * (d[index]) + (lambda * (b));

            if (d[index] < 0) { d[index] = 0; }

            /*Console.Write("[");
            for (int i = 0; i < size; i++)
                Console.Write(((int)(d[i] * 100)).ToString() + " ");
            Console.WriteLine("] ");*/

        }

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
