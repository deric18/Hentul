namespace Hentul.UT
{
    using Hentul;
    using System.Diagnostics;

    public class ScreenGrabberTest
    {
        ScreenGrabber sg;
        Random rand;

        [SetUp]
        public void Setup()
        {
           rand = new Random();
        }        

        
        public void MultipleInstanceTest()
        {
            int count = 3;
            sg = new ScreenGrabber(count);

            for( int i=0; i< count; i++ ) 
            {
                for(int j=0;j< count; j++ )
                {
                    for(int k=0; k< count; k++ )
                    {
                        Assert.That(sg.somBBM[i, j, k].ApicalLineArray.Length, Is.EqualTo(100));

                        Assert.AreEqual(4, sg.somBBM[i, j, k].Columns[rand.Next(0, 9), rand.Next(0, 9)].Neurons[rand.Next(0, 9)].dendriticList.Count);

                        Assert.AreEqual(4, sg.somBBM[i, j, k].Columns[rand.Next(0, 9), rand.Next(0, 9)].Neurons[rand.Next(0, 9)].AxonalList.Count);

                        Assert.IsNotNull(sg.somBBM[i, j, k].Columns[rand.Next(0, 9), rand.Next(0, 9)].Neurons[rand.Next(0, 9)].dendriticList.ElementAt(k));

                        Assert.IsNotNull(sg.somBBM[i, j, k].Columns[rand.Next(0, 9), rand.Next(0, 9)].Neurons[rand.Next(0, 9)].AxonalList.ElementAt(k));

                    }                    
                }                
            }            
        }
    }
}