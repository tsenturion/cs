namespace MathOperations
{
    namespace MathOperations.Core
    {
        public interface IMathOperation
        {
            public double Execute(double a, double b);
            
            string OperationName { get; }
        }

        public interface IAdvancedMath
        {
            double Power(double x, double y);
        }

        public abstract class OperationValidator
        {
            public abstract bool Validate(double a, double b);
            public virtual string ErrorMessage { get; }
        }
    }

    namespace MathOperations.Implementations
    {
        public class BasicCalculator : Core.IMathOperation
        {
            public String OperationName { get; private set; }

            public Double Execute(Double a, Double b)
            {
                OperationName = "Addition";
                return a + b;
            }

            public double Multiply(double a, double b)
            {
                OperationName = "Multiply";
                return a * b;
            }
        }

        public class ScientificCalculator : Core.IMathOperation, Core.IAdvancedMath
        {
            public String OperationName { get; private set; }

            public Double Execute(Double a, Double b)
            {
                OperationName = "Multiply";
                return a * b;
            }

            public Double Power(Double x, Double y)
            {
                OperationName = "Power";
                return Math.Pow(x, y);
            }

            internal double Logarithm(double x)
            {
                OperationName = "Logarithm";
                return Math.Log(x);
            }
        }

        public class SafeCalculator : Core.OperationValidator, Core.IMathOperation
        {
            public String OperationName => "Division";

            public Double Execute(Double a, Double b)
            {
                if (Validate(a, b)) return a / b;
                else throw new DivideByZeroException();
            }

            public override Boolean Validate(Double a, Double b)
            {
                return a / b != 0;
            }
        }
    }

    namespace MathOperations.Utilities
    {
        public class MathFormatter
        {
            public int FormatResult(double a)
            {
                string[] strs = a.ToString().Split('.');
                return int.Parse(strs[0]);
            }

            public double FormatWithPrecision(double a, int decimals = 2)
            {
                string[] strs = a.ToString().Split('.');
                string str = strs[0] + strs[1] + strs[2].Remove(strs[2].Length - decimals);
                return double.Parse(str);
            }

            [Obsolete(message: "Use FormatResult()")]
            public int OldCalculateFormat(double a)
            {
                return int.Parse(a.ToString());
            }

            public double FormatWithCulture(double a, int decimals = 2, bool abs = false)
            {
                double result = FormatWithPrecision(a, decimals);
                if (abs) result = Math.Abs(result);
                return result;
            }
        }

        internal static class Constants
        {
            static double Pi = Math.PI;
            static double E = Math.E;
        }
    }
}
