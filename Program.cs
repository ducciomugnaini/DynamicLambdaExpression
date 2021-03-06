using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DynamicLambdaExpression
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            //---------------------------------------------------------------------
            //--------------- Static Lambda Exmpression ---------------------------
            //---------------------------------------------------------------------

            IEnumerable<TennisPlayer> tennisPlayers = Seed();
            var staticLambdaResult = tennisPlayers.Where(tp => tp.VictoryAmount > 19400000);

            Print("Static Result", staticLambdaResult);

            //---------------------------------------------------------------------
            //--------------- Dynamic Lambda Exmpression --------------------------
            //---------------------------------------------------------------------

            var parameterType = Expression.Parameter(Type.GetType("DynamicLambdaExpression.TennisPlayer"), "tp");
            var parameterProperty = Expression.Property(parameterType, "VictoryAmount");
            var constant = Expression.Constant(19400000.0);
            var expression = Expression.GreaterThan(parameterProperty, constant); // tp.VictoryAmount > 19400000

            var lambda = Expression.Lambda<Func<TennisPlayer, bool>>(expression, parameterType); // tp => tp.VictoryAmount > 19400000
            var compiledLambda = lambda.Compile();
            var dynamicLambdaResult = tennisPlayers.Where(compiledLambda);

            Print("Dynamic Result", dynamicLambdaResult);

            //---------------------------------------------------------------------
            //--------------- Dynamic String Lambda Exmpression -------------------
            //---------------------------------------------------------------------

            var stringFilter = "tp => tp.VictoryAmount > 19400000";
            var options = ScriptOptions.Default.AddReferences(typeof(Program).Assembly);
            
            Func<TennisPlayer, bool> tennisPlayerFiler = await CSharpScript.EvaluateAsync<Func<TennisPlayer, bool>>(stringFilter, options);

            var dynamicStringLambdaResult = tennisPlayers.Where(tennisPlayerFiler);

            Print("Dynamic String Lambda Result", dynamicStringLambdaResult);

            //---------------------------------------------------------------------
            //--------------- Dynamic Lambda Exmpression --------------------------
            //---------------------------------------------------------------------
            /* A LambdaExpression has two components:
                - a parameter list—(string x)—represented by the Parameters property
                - a body—x.StartsWith("a")—represented by the Body property.
             */

            /*var asd = GetExtensionMethods(typeof(Program).Assembly, typeof(double)).ToList();

            var elementType = Type.GetType("DynamicLambdaExpression.TennisPlayer");

            var parameterType_1 = Expression.Parameter(Type.GetType("DynamicLambdaExpression.TennisPlayer"), "tp");
            var constant_1 = Expression.Constant(19400000.0);
            Expression body = Expression.Call(
                Expression.Property(
                    parameterType_1,
                    elementType.GetProperties().FirstOrDefault(p => p.Name.Equals("VictoryAmount"))),
                    GetExtensionMethods(typeof(Program).Assembly, typeof(double)).ToList()[0],
                constant_1
            );

            Expression<Func<TennisPlayer, bool>> expr = Expression.Lambda<Func<TennisPlayer, bool>>(body, parameterType_1);
            var dynamicLambdaResult_1 = tennisPlayers.AsQueryable().Where(expr);

            Print("Dynamic Result with Body Method", dynamicLambdaResult_1);*/

            return 1;
        }

        private static List<TennisPlayer> Seed()
        {
            return new List<TennisPlayer>
            {
                new TennisPlayer()
                {
                    Name = "Rafael",
                    Surname = "Nadal",
                    Age = 35,
                    VictoryAmount = 15400000
                },
                new TennisPlayer()
                {
                    Name = "Roger",
                    Surname = "Federer",
                    Age = 37,
                    VictoryAmount = 37640000
                }
            };
        }

        private static void Print(string title, IEnumerable<TennisPlayer> tennisPlayers)
        {
            Console.WriteLine(title);
            foreach (var tp in tennisPlayers)
            {
                Console.WriteLine($"{tp.Name} {tp.Surname} | {tp.VictoryAmount}");
            }
        }

        static IEnumerable<MethodInfo> GetExtensionMethods(Assembly assembly,
        Type extendedType)
        {
            var query = from type in assembly.GetTypes()
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static
                            | BindingFlags.Public | BindingFlags.NonPublic)
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where method.GetParameters()[0].ParameterType == extendedType
                        select method;
            return query;
        }
    }

    public class TennisPlayer
    {
        public String Name { get; set; }
        public String Surname { get; set; }
        public int Age { get; set; }
        public double VictoryAmount { get; set; }
    }

    public static class DoubleExtension
    {
        public static bool GreaterThan(this double a, double b)
        {
            return a > b;
        }
    }


}
