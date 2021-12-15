using System;
using System.Collections.Generic;
using System.Text;

namespace CTA.WebForms.Tests.TestingArea.TestFiles
{
    public class TestClassFile
    {
        private readonly int myInt;

        public TestClassFile()
        {
            myInt = 5;
        }

        public int GetMyIntSq()
        {
            return myInt * myInt;
        }
    }
}
