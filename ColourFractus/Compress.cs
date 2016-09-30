using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
namespace ColourFractus
{
    public class Compress
    {
        private static ImageDescription ImageDescription; // Описание изображения         
        int rankSize; // размер ранга 
        int domainSize; // разме домена 
        double compressionRatio; // степень сжатия
        int measurementError; // допустимое отклонение 
        Bitmap bitmap; 
        int width; // высота изображения
        int height; // высота изображения
        int rankXNum; // количество рангов по горизонтали 
        int rankYNum; // количество рангов по вертикали 
        int domainXNum; // количество доненов по горизонтали 
        int domainYNum; // количество доненов по вертикали
        int areaRank;
        static int[,] Red; // Массив оттенков красного для пикселей 
        static int[,] Green;// Массив оттенков зеленого для пикселей 
        static int[,] Blue;// Массив оттенков синего для пикселей 
        Dictionary<Domain,int> DictionarySaveDomain = new Dictionary<Domain,int>();
        List<Domain> listSaveDomain = new List<Domain>();
        RankList rankList = new RankList();
        DomainList domainList = new DomainList();
        private Object _lock = new Object();
        public Compress(Bitmap bitmap, int rankSize, double compressionRatio, int measurementError) // конструктор для задания начальных параметров
        {
            this.bitmap = bitmap;
            this.rankSize = rankSize;
            domainSize = rankSize * 2;
            areaRank = rankSize * rankSize;
            this.compressionRatio = compressionRatio;
            this.measurementError = measurementError;
            width = bitmap.Width;           
            height = bitmap.Height;
            ImageDescription.width = width;
            ImageDescription.height = height;
            ImageDescription.rankSize = rankSize;
            rankXNum = width / rankSize;
            rankYNum = height / rankSize;
            domainXNum = width / domainSize;
            domainYNum = height / domainSize;
            Red = new int[width, height];
            Green = new int[width, height];
            Blue = new int[width, height];
            ImageDescription.rankInfoRed = new RankInfo[rankXNum, rankYNum];
            ImageDescription.rankInfoGreen = new RankInfo[rankXNum, rankYNum];
            ImageDescription.rankInfoBlue = new RankInfo[rankXNum, rankYNum];
        }
        public ImageDescription CompressStart() // Сжимаем изображение, поиском наилучшего доменного блока для каждого рангового
        {            
            for (int i = 0; i < width; i++)
            {
                for (int k = 0; k < height; k++)
                {
                    Red[i, k] = bitmap.GetPixel(i, k).R;
                    Green[i, k] = bitmap.GetPixel(i, k).G;
                    Blue[i, k] = bitmap.GetPixel(i, k).B;
                }
            }           
            Parallel.For(1, 7, i =>
            {
                switch (i)
                {
                    case 1:
                        rankList.Red = GetRank(ref rankList.sumRed,  Red);
                        break;
                    case 2:
                        domainList.Red = GetDomain(ref domainList.sumRed, Red);
                        break;
                    case 3:
                        rankList.Green = GetRank(ref rankList.sumGreen, Green);
                        break;
                    case 4:
                        domainList.Green = GetDomain(ref domainList.sumGreen,  Green);
                        break;
                    case 5:
                        rankList.Blue = GetRank(ref rankList.sumBlue, Blue);
                        break;
                    case 6:
                        domainList.Blue = GetDomain(ref domainList.sumBlue, Blue);
                        break;
                }

            });
            Parallel.For(1, 4, i =>         
            {
                switch (i)
                {
                    case 1:
                        ParallelMethod(ImageDescription.rankInfoRed, domainList.Red, domainList.sumRed, rankList.Red, rankList.sumRed);
                        break;
                    case 2:
                        ParallelMethod(ImageDescription.rankInfoGreen, domainList.Green, domainList.sumGreen, rankList.Green, rankList.sumGreen);
                        break;
                    case 3:
                        ParallelMethod(ImageDescription.rankInfoBlue, domainList.Blue, domainList.sumBlue, rankList.Blue, rankList.sumBlue);
                        break;
                }
            });
            foreach(KeyValuePair<Domain,int> item in  DictionarySaveDomain.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Reverse())
            {
                if (listSaveDomain.Count < 240 & item.Value > 3)
                {
                    listSaveDomain.Add(item.Key);
                }
                else 
                {
                    break;
                }
            }
            ImageDescription.listSaveDomain = listSaveDomain;
            return ImageDescription;
        }
        public void ParallelMethod(RankInfo[,] rankInfo, List<List<int[,]>> domain, List<int> domainSum, List<int[,]> rank, List<int> rankSum)
        {            
            Parallel.For(0, rankXNum * rankYNum, indexrankBlock =>
            {
                double minDistance = double.MaxValue;
                int domainXSave = 0;
                int domainYSave = 0;
                int colorSave = 0;
                int rotTypeSave = 0;
                int color = 0;
                bool check = true;
                double distance = 0;
                for (int indexDomain = 0; indexDomain < domainXNum * domainYNum; indexDomain++)
                {
                    if (check)
                    {                        
                        color = (int)(rankSum[indexrankBlock] - compressionRatio * domainSum[indexDomain]) / areaRank;                        
                        foreach (int[,] ArrayDomainRot in domain[indexDomain])
                        {
                            Distance(out distance, minDistance, rank[indexrankBlock], ArrayDomainRot, color, rankSize, compressionRatio); // метод находжения дистанции между ранговым и доменным блоком.
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                domainXSave = (indexDomain / domainYNum);
                                domainYSave = (indexDomain % domainYNum);
                                colorSave = color;
                                rotTypeSave = domain[indexDomain].IndexOf(ArrayDomainRot);
                            }
                            if (distance < measurementError)
                            {
                                check = false;
                                break;
                            }
                        }                     
                    }
                    else { break; }
                }
                int rankX = indexrankBlock / rankYNum; // Находим номер в двумерном массиве
                int rankY = indexrankBlock % rankYNum; // Находим номер в двумерном массиве
                rankInfo[rankX, rankY].domain = new Domain(domainXSave, domainYSave, rotTypeSave);
                rankInfo[rankX, rankY].color = colorSave;
                lock (_lock)
                {
                    if (DictionarySaveDomain.ContainsKey(rankInfo[rankX, rankY].domain))
                    {
                        DictionarySaveDomain[rankInfo[rankX, rankY].domain]++;
                    }
                    else
                    {
                        DictionarySaveDomain.Add(rankInfo[rankX, rankY].domain, 0);
                    }
                }
            });
        }
        
        private List<int[,]> GetRank(ref List<int> rankListSum, int[,] pixels) //метода для создания списка списков массивов
        {
            List<int[,]> rankList = new List<int[,]>();
            rankListSum = new List<int>();
            int rankXX;
            int rankYY;
            int Sum;
            int[,] rankArray;
            for (int rankX = 0; rankX < rankXNum; rankX++)
            {
                for (int rankY = 0; rankY < rankYNum; rankY++)
                {
                    rankArray = new int[rankSize, rankSize];
                    rankXX = rankX * rankSize;
                    rankYY = rankY * rankSize;
                    Sum = 0;
                    for (int i = 0; i < rankSize; i++)
                    {
                        for (int j = 0; j < rankSize; j++)
                        {
                            rankArray[i, j] = pixels[rankXX + i, rankYY + j];
                            Sum += rankArray[i, j];
                        }
                    }
                    rankListSum.Add(Sum);
                    rankList.Add(rankArray);                   
                }
            }
            return rankList;
        }
        private List<List<int[,]>> GetDomain(ref List<int> domainListSum, int[,] pixels) //метода для создания списка списков массивов
        {
            int Sum;
            int domainXX;
            int domainYY;
            List<List<int[,]>> domainList = new List<List<int[,]>>();
            List<int[,]> domainList2;
            domainListSum = new List<int>();
            int[,] numArray2;
            for (int domainX = 0; domainX < domainXNum; domainX++)
            {
                for (int domainY = 0; domainY < domainYNum; domainY++)
                {
                    domainList2 = new List<int[,]>();
                    domainXX = domainX * domainSize;
                    domainYY = domainY * domainSize;
                    numArray2 = new int[rankSize, rankSize]; // массив цветов пикселей доменного блока сжатый до размеров рангового
                    Sum = 0;                    
                    for (int i = 0; i < rankSize; i++)
                    {
                        for (int j = 0; j < rankSize; j++)
                        {
                            numArray2[i, j] = (((pixels[domainXX + i * 2, domainYY + j * 2] + pixels[domainXX + (i * 2) + 1, domainYY + j * 2]) + pixels[domainXX + i * 2, domainYY + (j * 2) + 1]) + pixels[domainXX + (i * 2) + 1, domainYY + (j * 2) + 1]) / 4;
                            // доменный блок сжали до размеров рангового, находя среднее значение соседних пискелей 
                            Sum += numArray2[i, j];                           
                        }
                    }
                    domainListSum.Add(Sum);                   
                    for (int rotType = 0; rotType < 8; rotType++) // перебераем 8 положений доменного блока 0, 90, 180, 270 градусов и зеркальный отражения этих поворотов
                    {
                        int[,] numArray3 = new int[rankSize, rankSize];
                        for (int i = 0; i < rankSize; i++)
                        {
                            for (int j = 0; j < rankSize; j++)
                            {
                                switch (rotType)
                                {
                                    case 0:
                                        numArray3[i, j] = numArray2[i, j]; //1   // 1 - |1-polar|  
                                        break;

                                    case 1:
                                        numArray3[i, j] = numArray2[(rankSize - j) - 1, i]; //2 // 2 - |1-polar|   
                                        break;

                                    case 2:
                                        numArray3[i, j] = numArray2[(rankSize - i) - 1, (rankSize - j) - 1]; //4 // 4 - |1-polar| 
                                        break;

                                    case 3:
                                        numArray3[i, j] = numArray2[j, (rankSize - i) - 1]; //3
                                        break;

                                    case 4:
                                        numArray3[i, j] = numArray2[(rankSize - i) - 1, j];//2
                                        break;

                                    case 5:
                                        numArray3[i, j] = numArray2[(rankSize - j) - 1, (rankSize - i) - 1]; // 4
                                        break;

                                    case 6:
                                        numArray3[i, j] = numArray2[i, (rankSize - j) - 1]; //3
                                        break;

                                    case 7:
                                        numArray3[i, j] = numArray2[j, i]; // 1
                                        break;
                                }
                            }
                        }
                        domainList2.Add(numArray3); // занесли 8 массивов в список 
                    }
                    domainList.Add(domainList2); // Занесли список о восьми положениях доменного блока в список
                }
            }
            return domainList;
        }
        private void Distance(out double distance,double d, int[,] rankArray, int[,] domainAvgP, int color, int rankSize, double ratio)
        {
            distance = 0;
            double a = 0;
            for (int i = 0; i < rankSize; i++)
            {
                for (int j = 0; j < rankSize; j++)
                {
                    a = ratio * domainAvgP[i, j] + color - rankArray[i, j];
                    distance += a * a;
                    if (d < distance)
                    {
                        break;
                    }
                }
                if (d < distance)
                {
                    break;
                }
            }
        }
    }
}
