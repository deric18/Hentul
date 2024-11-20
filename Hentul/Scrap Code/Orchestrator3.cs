namespace Hentul
{
    using FirstOrderMemory.Models;
    using FirstOrderMemory.BehaviourManagers;
    using System.Configuration;
    using System.Diagnostics;
    using Common;
    using FirstOrderMemory.Models.Encoders;
    using SixLabors.ImageSharp;
    using System.Drawing;

    internal class Orchestrator3
    {
        #region Used Variables

        public int NumPixelsToProcessPerBlock { get; private set; }

        public int numPixelsProcessedPerBBM;

        public int NumBBMNeeded { get; private set; }

        private bool LogMode { get; set; }

        public bool IsMock { get; private set; }

        int NumColumns, Z;

        public Dictionary<int, List<Position_SOM>> Buckets;

        public BlockBehaviourManager[] fomBBM { get; private set; }

        public BlockBehaviourManager somBBM { get; private set; }

        public int[] MockBlockNumFires { get; private set; }

        private bool devbox = false;

        public int BlockOffset;

        public int UnitOffset;

        public int ImageIndex { get; private set; }

        public ulong CycleNum;

        public List<string> ImageList { get; private set; }

        public int blackPixelCount = 0;

        public Bitmap bmp;

        private readonly int FOMLENGTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMLENGTH"]);
        private readonly int FOMWIDTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMWIDTH"]);
        private readonly int SOM_NUM_COLUMNS = Convert.ToInt32(ConfigurationManager.AppSettings["SOMNUMCOLUMNS"]);
        private readonly int SOM_COLUMN_SIZE = Convert.ToInt32(ConfigurationManager.AppSettings["SOMCOLUMNSIZE"]);


        #endregion

        public Orchestrator3(int range, bool isMock = false, bool ShouldInit = true, int mockImageIndex = 7)
        {
            //Todo : Project shape data of the input image to one region and project colour data of the image to another region.                        

            NumPixelsToProcessPerBlock = range;

            numPixelsProcessedPerBBM = 20;

            LogMode = false;

            double numerator = (2 * NumPixelsToProcessPerBlock) * (2 * NumPixelsToProcessPerBlock);

            double denominator = numPixelsProcessedPerBBM;

            double value = (numerator / denominator);

            bool b = value % 1 != 0;

            if (value % 1 != 0)
            {
                throw new InvalidDataException("Number Of Pixels should always be a factor of BucketColLength : NumPixels : " + NumPixelsToProcessPerBlock.ToString());
            }

            BlockOffset = range * 2;

            UnitOffset = 10;

            NumBBMNeeded = (int)value;

            fomBBM = new BlockBehaviourManager[NumBBMNeeded];

            int x1 = 10 * NumBBMNeeded;

            somBBM = new BlockBehaviourManager(x1, 10, 4);

            NumColumns = 10;

            IsMock = isMock;

            Z = 10;

            CycleNum = 0;

            for (int i = 0; i < NumBBMNeeded; i++)
            {
                fomBBM[i] = new BlockBehaviourManager(NumColumns, NumColumns, Z, BlockBehaviourManager.LayerType.Layer_4 ,BlockBehaviourManager.LogMode.BurstOnly);
            }

            if (isMock)
                ImageIndex = mockImageIndex;
            else
                ImageIndex = 0;
                

            MockBlockNumFires = new int[NumBBMNeeded];


            ImageList = AddAllTheFruits();



            LoadFOMnBOM();

        }

        public void LoadFOMnBOM()
        {

            Console.WriteLine("Starting Initialization  of FOM objects : \n");


            for (int i = 0; i < NumBBMNeeded; i++)
            {
                fomBBM[i].Init(10);
            }


            Console.WriteLine("Finished Init for this Instance \n");
            Console.WriteLine("Range : " + NumPixelsToProcessPerBlock.ToString() + "\n");
            Console.WriteLine("Total Number of Pixels :" + (NumPixelsToProcessPerBlock * NumPixelsToProcessPerBlock * 4).ToString() + "\n");
            Console.WriteLine("Total First Order BBMs Created : " + NumBBMNeeded.ToString() + "\n");


            Console.WriteLine("Initing SOM Instance now ... \n");

            somBBM.Init(1);

            Console.WriteLine("Finished Init for SOM Instance , Total Time ELapsed : \n");

            Console.WriteLine("Finished Initting of all Instances, System Ready!" + "\n");
        }

        private List<string> AddAllTheFruits()
        {
            var dict = new List<string>();

            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\Apple.png");
            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\Ananas.png");
            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\orange.png");
            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\bannana.jpg");
            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\grapes.png");
            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\jackfruit.png");
            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\watermelon.png");
            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\white.png");

            return dict;
        }

        public void GrabNProcess(ref bool[,] booleans)          //We process one image at once.
        {
            //Todo : Pixel combination should not be serial , it should be randomly distributed through out the unit

            Stopwatch stopWatch = new Stopwatch();

            stopWatch.Start();

            int TotalReps = 2;

            int TotalNumberOfPixelsToProcess_X = GetRoundedTotalNumberOfPixelsToProcess(bmp.Width);
            int TotalNumberOfPixelsToProcess_Y = GetRoundedTotalNumberOfPixelsToProcess(bmp.Height);

            int TotalPixelsCoveredPerIteration = BlockOffset * BlockOffset; //2500            

            int num_blocks_per_bmp_x = (int)(TotalNumberOfPixelsToProcess_X / BlockOffset);
            int num_blocks_per_bmp_y = (int)(TotalNumberOfPixelsToProcess_Y / BlockOffset);

            int num_unit_per_block_x = 5;
            int num_unit_per_block_y = 5;

            int num_pixels_per_Unit_x = 10;
            int num_pixels_per_Unit_y = 10;

            using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(ImageList[ImageIndex++]))
            {

                for (int reps = 0; reps < TotalReps; reps++)
                {
                    for (int blockid_y = 0; blockid_y < num_blocks_per_bmp_y; blockid_y++)
                    {
                        for (int blockid_x = 0; blockid_x < num_blocks_per_bmp_x; blockid_x++)
                        {
                            int bbmId = 0;

                            for (int unitId_y = 0; unitId_y < num_unit_per_block_y; unitId_y++)
                            {
                                for (int unitId_x = 0; unitId_x < num_unit_per_block_x; unitId_x++)
                                {
                                    BoolEncoder boolEncoder = new BoolEncoder(100, 20);

                                    for (int j = 0; j < num_pixels_per_Unit_x; j++)
                                    {
                                        for (int i = 0; i < num_pixels_per_Unit_y; i++)
                                        {
                                            int pixel_x = blockid_x * BlockOffset + unitId_x * UnitOffset + i;
                                            int pixel_y = blockid_y * BlockOffset + unitId_y * UnitOffset + j;

                                            //if the pixel is Black then tag the pixel location

                                            if (blockid_x == 6 && blockid_y == 0 && unitId_x == 4 && unitId_y == 2 && j == 2)
                                            {
                                                int bp = 1;
                                            }

                                            if (CheckifPixelisBlack(pixel_x, pixel_y))
                                            {

                                                var dataToEncode = (j % 2).ToString() + "-" + i.ToString();
                                                boolEncoder.SetEncoderValues(dataToEncode);

                                            }
                                        }

                                        if (j % 2 == 1)     //Bcoz one BBM covers 2 lines of pixel per unit
                                        {
                                            if (fomBBM[bbmId].TemporalLineArray[0, 0] == null)
                                            {
                                                fomBBM[bbmId].Init(bbmId);
                                            }

                                            if (boolEncoder.HasValues())
                                            {
                                                CycleNum++;

                                                var imageSDR = boolEncoder.Encode(iType.SPATIAL);

                                                fomBBM[bbmId++].Fire(imageSDR);

                                                SDR_SOM fomSDR = fomBBM[bbmId].GetPredictedSDR();

                                                if (fomSDR != null && fomSDR.ActiveBits.Count != 0)
                                                {
                                                    fomSDR = AddSOMOverheadtoFOMSDR(fomSDR, blockid_x, blockid_y);

                                                    somBBM.Fire(fomSDR);
                                                }

                                            }

                                            boolEncoder.ClearEncoderValues();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            PrintMoreBlockVitals();


            BackUp();

            Console.WriteLine("Finished Processing Pixel Values : Total Time Elapsed in seconds : " + (stopWatch.ElapsedMilliseconds / 1000).ToString());

            Console.WriteLine("Black Pixel Count :: " + blackPixelCount.ToString());

            Console.WriteLine("Done Processing Image");

            Console.Read();
        }

        private SDR_SOM AddSOMOverheadtoFOMSDR(SDR_SOM fomSDR, int blockidX, int blockIdY)
        {
            SDR_SOM toRet;
            int block_offset = 10;
            List<Position_SOM> newPosList = new List<Position_SOM>();

            foreach (var pos in fomSDR.ActiveBits)
            {
                newPosList.Add(new Position_SOM(blockidX * block_offset + pos.X, pos.Y));
            }

            toRet = new SDR_SOM(1250, 10, newPosList, iType.SPATIAL);

            return toRet;
        }

        public bool CheckifPixelisBlack(int x, int y)
        {
            if (x >= bmp.Width || y >= bmp.Height)
                return false;

            var color = bmp.GetPixel(x, y);

            return (color.R < 200 && color.G < 200 && color.B < 200);
        }

        public int GetRoundedTotalNumberOfPixelsToProcess(int numberOfPixels_Index)
        {
            if (numberOfPixels_Index % BlockOffset == 0)
            {
                return numberOfPixels_Index;
            }

            int nextMinNumberOfPixels = numberOfPixels_Index;

            int halfOfnextMinNumberOfPixels = numberOfPixels_Index / 2;

            while (nextMinNumberOfPixels % 50 != 0)
            {
                nextMinNumberOfPixels--;

                if (nextMinNumberOfPixels < halfOfnextMinNumberOfPixels)
                {
                    Console.WriteLine(" GetRoundedTotalNumberOfPixelsToProcess() :: Unable to find the proper lower Bound");
                }

            }

            if (nextMinNumberOfPixels % 50 != 0)
                throw new InvalidDataException("Grab :: blockLength should always be factor of NumPixelToProcess");

            return nextMinNumberOfPixels;
        }

        private void PrintMoreBlockVitals()
        {
            Console.WriteLine("Enter '1' to see a list of all the Block Usage List :");

            int w = Console.Read();

            if (w == 49)
            {
                ulong totalIncludedCycle = 0;

                for (int i = 0; i < fomBBM.Count(); i++)
                {
                    if (fomBBM[i].BBMID != 0)
                        Console.WriteLine(i.ToString() + " :: Block ID : " + fomBBM[i].PrintBlockDetailsSingleLine() + " | " + "Inclusded Cycle: " + fomBBM[i].CycleNum.ToString());

                    totalIncludedCycle += fomBBM[i].CycleNum;

                }

                Console.WriteLine("Total Participated Cycles : " + totalIncludedCycle);
                Console.WriteLine("Orchestrator CycleNum : " + CycleNum.ToString());

                if (totalIncludedCycle != CycleNum)
                {
                    Console.WriteLine("ERROR : Incorrect Cycle Distribution amoung blocks");
                    Thread.Sleep(5000);
                }
            }
        }

        public void BackUp()
        {
            for (int i = 0; i < fomBBM.Length; i++)
            {
                fomBBM[i].BackUp(i.ToString());
            }

            somBBM.BackUp("SOM-1");
        }
    }
}
