using MathNet.Symbolics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Expr = MathNet.Symbolics.SymbolicExpression;

namespace CallaghanDev.Utilities.MathTools
{
    
    public static class Function
    {
        private static readonly Dictionary<string, Expr> ExpressionCache = new Dictionary<string, Expr>();

        public static float EvaluateExpression(string expression, string variableName, float variableValue)
        {
            // Check if the expression has been parsed before
            if (!ExpressionCache.TryGetValue(expression, out Expr parsedExpression))
            {
                // Parse the expression and add to cache
                parsedExpression = Expr.Parse(expression);
                ExpressionCache[expression] = parsedExpression;
            }

            // Create a dictionary to hold the variable substitution
            var substitution = new Dictionary<string, FloatingPoint> { { variableName, variableValue } };

            // Evaluate the parsed expression using the substitution
            var evaluationResult = parsedExpression.Evaluate(substitution);

            // Convert the result to a float and return
            return (float)evaluationResult.RealValue;
        }

        public static float sigmoid(float x)//activation functions and their corrosponding derivatives
        {
            float k = (float)Math.Exp(-x);
            return 1 / (1.0f + k);
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
                return Function.sigmoid(x) * (1 - Function.sigmoid(x));
            }
            public static double tanh(double x)
            {
                return 1 - (x * x);
            }
            public static double relu(double x)
            {
                return (x >= 0) ? 1 : 0;
            }
            public static double leakyrelu(double x)
            {
                return (0 >= x) ? 0.01d : 1;
            }
            public static float sigmoid(float x)
            {
                return Function.sigmoid(x) * (1 - Function.sigmoid(x));
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
            public static float EvaluateFirstDerivative(string expression, string variableName, float variableValue)
            {
                // Check if the derivative has been calculated and cached before
                if (!ExpressionCache.TryGetValue("D_" + expression + "_" + variableName, out Expr derivativeExpression))
                {
                    // Parse the main expression if not already cached
                    if (!ExpressionCache.TryGetValue(expression, out Expr parsedExpression))
                    {
                        parsedExpression = Expr.Parse(expression);
                        ExpressionCache[expression] = parsedExpression;
                    }

                    // Differentiate the parsed expression and add to cache
                    derivativeExpression = parsedExpression.Differentiate(variableName);
                    ExpressionCache["D_" + expression + "_" + variableName] = derivativeExpression;
                }

                // Create a dictionary to hold the variable substitution
                var substitution = new Dictionary<string, FloatingPoint> { { variableName, variableValue } };

                // Evaluate the derivative expression using the substitution
                var derivativeResult = derivativeExpression.Evaluate(substitution);

                // Convert the result to a float and return
                return (float)derivativeResult.RealValue;
            }
        }
    }

}
