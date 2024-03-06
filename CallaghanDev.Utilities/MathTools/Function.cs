using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.MathTools
{
    public static class Function
    {
        public static float sigmoid(float x)//activation functions and their corrosponding derivatives
        {
            float k = (float)Math.Exp(x);
            return k / (1.0f + k);
        }
        public static float tanh(float x)
        {
            return (float)Math.Tanh(x);
        }

        //Rectified Linear Unit
        public static double relu(double x)
        {
            return (0 >= x) ? 0 : x;
        }
        public static float relu(float x)
        {
            return (0 >= x) ? 0 : x;
        }

        // Leaky Rectified Linear Unit (Leaky ReLU)
        public static double leakyrelu(double x)
        {
            return (0 >= x) ? 0.01d * x : x;
        }
        public static float leakyrelu(float x)
        {
            return (0 >= x) ? 0.01f * x : x;
        }

        public static double sigmoid(double x)
        {
            double k = Math.Exp(x);
            return k / (1.0d + k);
        }
        public static double tanh(double x)
        {
            return Math.Tanh(x);// This is not a good move, lose the precision of the decimal by converting to a double, and highly inefficent
        }

        public static class FirstDerivative
        {
            public static double sigmoid(double x)
            {
                return x * (1 - x);
            }
            public static double tanh(double x)
            {
                return 1 - (x * x);
            }
            public static double relu(double x)
            {
                return (0 >= x) ? 0 : 1;
            }
            public static double leakyrelu(double x)
            {
                return (0 >= x) ? 0.01d : 1;
            }
            public static float sigmoid(float x)
            {
                return x * (1 - x);
            }
            public static float tanh(float x)
            {
                return 1 - (x * x);
            }
            public static float relu(float x)
            {
                return (0 >= x) ? 0 : 1;
            }
            public static float leakyrelu(float x)
            {
                return (0 >= x) ? 0.01f : 1;
            }
        }
    }

    public static class UtilityFunctions
    {
        // Mean Squared Error Loss
        /*
        Purpose: Used as a loss function in regression problems. It measures the average of the squares of the errors—that is, 
            the average squared difference between the estimated values and the actual value.
        Usage: Commonly used in linear regression, neural networks for regression tasks, etc. It's helpful in optimization 
            problems where minimizing the error is the goal.
         */
        public static double mse(double actual, double predicted)
        {
            return Math.Pow(predicted - actual, 2);
        }

        // Cross-Entropy Loss for binary classification
        /*
         Purpose: A loss function used for binary classification problems. It measures the performance of a classification model whose
               output is a probability value between 0 and 1. Cross-entropy loss increases as the predicted probability diverges from the actual label.
         Usage: Widely used in models that output probabilities, such as logistic regression and binary classification neural networks.
         */
        public static double binaryCrossEntropy(double actual, double predicted)
        {
            return -(actual * Math.Log(predicted) + (1 - actual) * Math.Log(1 - predicted));
        }

        // Min-Max Normalization
        /*
        Purpose: Also known as Standard Score or Standardization, it is a normalization technique where the values are rescaled so that they have the
            properties of a standard normal distribution with μ = 0 and σ = 1, where μ is the mean and σ is the standard deviation.
        Usage: Useful in algorithms that assume the data is normally distributed, or in scenarios where the scale and distribution of the data might 
            affect the learning process.
        */
        public static double minMaxNormalize(double value, double min, double max)
        {
            return (value - min) / (max - min);
        }

        // Z-Score Normalization
        /*
        Purpose: Also known as Standard Score or Standardization, it is a normalization technique where the values are rescaled so that they have the 
            properties of a standard normal distribution with μ = 0 and σ = 1, where μ is the mean and σ is the standard deviation.
        Usage: Useful in algorithms that assume the data is normally distributed, or in scenarios where the scale and distribution of the data might 
            affect the learning process.
        */
        public static double zScoreNormalize(double value, double mean, double stdDev)
        {
            return (value - mean) / stdDev;
        }

        // Softmax Function
        /*
        Purpose: A function that converts a vector of values into a probability distribution, where the probabilities are proportional to the exponentials of the input
            numbers. Often used in the final layer of a neural network-based classifier to represent the probabilities of the classes.
        Usage: Predominantly used in multi-class classification problems, especially in neural networks to output probabilities for each class in classification tasks.
        */
        public static double[] softmax(double[] values)
        {
            double[] softmaxValues = new double[values.Length];
            double sumExp = 0.0;

            foreach (double value in values)
            {
                sumExp += Math.Exp(value);
            }
            for (int i = 0; i < values.Length; i++)
            {
                softmaxValues[i] = Math.Exp(values[i]) / sumExp;
            }
            return softmaxValues;
        }
    }
}
