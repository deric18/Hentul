using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SecondOrderMemroyUnitTest
{
    [TestClass]
    public class BlockBehaviourManagerTester
    {
        {OneTimeSetUp]
        public void Init()
        {

        }

        [TestMethod]
        public void TestTemporalLines()
        {
            for (int i = 0; i < NumColumns; i++)
            {
                for (int j = 0; j < NumColumns; j++)
                {
                    for (int k = 0; k < NumColumns; k++)
                    {
                        Console.WriteLine(i :  i, " ", j : j," ", k : k);
                    }
                    Console.WriteLine();
                }
            }
        }


        [TestMethod]
        public void TestApicalLines()
        {

        }
    }
}
