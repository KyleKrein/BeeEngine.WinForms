namespace BeeEngine.Neuron
{
    public sealed class SimpleNeuron
    {
        private decimal[] weights;
        private double weight;
        public double LastError { get; private set; }
        public double Smoothing { get; set; } = 0.0001d;
        
        public void Init(int NumberOfInputs)
        {
            if(NumberOfInputs == 1)
            {
                weight = MathU.RandomDouble();
            }
            else
            {
                weights = new decimal[NumberOfInputs];
                for (int i = 0; i < NumberOfInputs; i++)
                {
                    weights[i] = (decimal)MathU.RandomDouble();
                }
            }
            
        }
        //public void Init(decimal[] Weights)
        //{
        //    weights = Weights;
        //}
        public void Init(double Weight)
        {
            weight = Weight;
        }
        //public decimal ProcessInputData(decimal[] input)
        //{
        //    decimal output = 0;
        //    for (int i = 0; i < weights.Length; i++)
        //    {
        //        output += input[i] * weights[i];
        //    }
        //    return output;
        //}
        public double ProcessInputData(double input)
        {
            return weight*Math.Pow(Math.E, Math.Pow(-input, 2));//weight*Math.Log(1 + Math.Exp(input));
        }
        /*public void Train(decimal[] input, decimal exprectedResult)
        {
            var actualResult = ProcessInputData(input);
            LastError = exprectedResult - actualResult;
            var correction = LastError / actualResult * Smoothing;
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] += correction;
            }
        }*/
        public void Train(double input, double exprectedResult)
        {
            
            var actualResult = ProcessInputData(input);
            LastError = exprectedResult - actualResult;
            var correction = LastError / actualResult * Smoothing;
            weight += correction;
            
        }
        public string getWeights()
        {
            string s = "Weights: ";
            foreach (var item in weights)
            {
                s += $"{item}   ";
            }
            return s;
        }
        public string getWeight()
        {
            return $"Weights: {weight}";
        }
    }
}