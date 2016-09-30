using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
namespace ColourFractus
{
    public class Decompress
    {
        public ImageDescription imageDescription;
        int rankSize;
        int domainSize;
        int accuracyImage;
        double compressionRatio;
        string pathImage;
        int factor;
        public Decompress(string pathImage, int rankSize, int accuracyImage, double compressionRatio, int factor)
        {
            this.factor = factor;
            this.pathImage = pathImage;
            this.rankSize = rankSize * factor;
            domainSize = rankSize * 2;
            this.accuracyImage = accuracyImage;
            this.compressionRatio = compressionRatio;      
        }
        public void DecompressStart()
        {
            int width = imageDescription.width * factor;
            int height = imageDescription.height * factor;
            int[,] pixelsArrayRed = new int[width, height];
            int[,] pixelsArrayGreen = new int[width, height];
            int[,] pixelsArrayBlue = new int[width, height];
            int rankCountWidth = width / rankSize;
            int rankCountHeight = height / rankSize;
            factor = factor * domainSize;
            Parallel.For(0, accuracyImage, accuracy =>
            {
                for (int j = 0; j < rankCountWidth; j++)
                {
                    for (int k = 0; k < rankCountHeight; k++)
                    {
                        int[,] domainArrayRed = GetDomain(pixelsArrayRed, imageDescription.rankInfoRed[j, k].domain.domainX *  factor, imageDescription.rankInfoRed[j, k].domain.domainY * factor, rankSize, imageDescription.rankInfoRed[j, k].domain.rotType);
                        int[,] rankArrayRed = new int[rankSize, rankSize];
                        int[,] domainArrayGreen = GetDomain(pixelsArrayGreen, imageDescription.rankInfoGreen[j, k].domain.domainX * factor, imageDescription.rankInfoGreen[j, k].domain.domainY *factor, rankSize, imageDescription.rankInfoGreen[j, k].domain.rotType);
                        int[,] rankArrayGreen = new int[rankSize, rankSize];
                        int[,] domainArrayBlue = GetDomain(pixelsArrayBlue, imageDescription.rankInfoBlue[j, k].domain.domainX * factor, imageDescription.rankInfoBlue[j, k].domain.domainY * factor, rankSize, imageDescription.rankInfoBlue[j, k].domain.rotType);
                        int[,] rankArrayBlue = new int[rankSize, rankSize];
                        for (int x = 0; x < rankSize; x++)
                        {
                            for (int y = 0; y < rankSize; y++)
                            {
                                rankArrayRed[x, y] = (int)(compressionRatio * (domainArrayRed[x, y]) + imageDescription.rankInfoRed[j, k].color);
                                rankArrayGreen[x, y] = (int)(compressionRatio * (domainArrayGreen[x, y]) + imageDescription.rankInfoGreen[j, k].color);
                                rankArrayBlue[x, y] = (int)(compressionRatio * (domainArrayBlue[x, y]) + imageDescription.rankInfoBlue[j, k].color);
                            }
                        }
                        SetRank(pixelsArrayRed, rankArrayRed, j, k, rankSize);
                        SetRank(pixelsArrayGreen, rankArrayGreen, j, k, rankSize);
                        SetRank(pixelsArrayBlue, rankArrayBlue, j, k, rankSize);
                    }
                }
            });

            PixelFormat r = PixelFormat.Format24bppRgb;    
            Bitmap outbitmap = new Bitmap(width, height,r);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (pixelsArrayRed[x, y] > 255) { pixelsArrayRed[x, y] = 255; }
                    if (pixelsArrayRed[x, y] < 0) { pixelsArrayRed[x, y] = 0; }
                    if (pixelsArrayGreen[x, y] > 255) { pixelsArrayGreen[x, y] = 255; }
                    if (pixelsArrayGreen[x, y] < 0) { pixelsArrayGreen[x, y] = 0; }
                    if (pixelsArrayBlue[x, y] > 255) { pixelsArrayBlue[x, y] = 255; }
                    if (pixelsArrayBlue[x, y] < 0) { pixelsArrayBlue[x, y] = 0; }
                    outbitmap.SetPixel(x, y, Color.FromArgb(pixelsArrayRed[x, y], pixelsArrayGreen[x, y], pixelsArrayBlue[x, y])); // Рисуем изображения, выставляя значения цвета пикселя 
                }
            }        
            outbitmap.Save(pathImage + "Fract.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            outbitmap.Dispose();
        }
        private int[,] GetDomain(int[,] pixels, int domainX, int domainY, int rankSize, int rotType)
        {
            int[,] numArray2 = new int[rankSize, rankSize]; // массив цветов пикселей
            for (int i = 0; i < rankSize; i++)
            {
                for (int j = 0; j < rankSize; j++)
                {
                    numArray2[i, j] = (((pixels[domainX + i * 2, domainY + j * 2] + pixels[domainX + (i * 2) + 1, domainY + j * 2]) + pixels[domainX + i * 2, domainY + (j * 2) + 1]) + pixels[domainX + (i * 2) + 1, domainY + (j * 2) + 1]) / 4;
                }
            }
            int[,] numArray3 = new int[rankSize, rankSize];
            for (int i = 0; i < rankSize; i++)
            {
                for (int j = 0; j < rankSize; j++)
                {
                    switch (rotType)
                    {
                        case 0:
                            numArray3[i, j] = numArray2[i, j];
                            break;

                        case 1:
                            numArray3[i, j] = numArray2[(rankSize - j) - 1, i];
                            break;

                        case 2:
                            numArray3[i, j] = numArray2[(rankSize - i) - 1, (rankSize - j) - 1];
                            break;

                        case 3:
                            numArray3[i, j] = numArray2[j, (rankSize - i) - 1];
                            break;

                        case 4:
                            numArray3[i, j] = numArray2[(rankSize - i) - 1, j];
                            break;

                        case 5:
                            numArray3[i, j] = numArray2[(rankSize - j) - 1, (rankSize - i) - 1];
                            break;

                        case 6:
                            numArray3[i, j] = numArray2[i, (rankSize - j) - 1];
                            break;

                        case 7:
                            numArray3[i, j] = numArray2[j, i];
                            break;
                    }
                }
            }
            return numArray3;
        }

        private void SetRank(int[,] pixels, int[,] rank, int rankX, int rankY, int rankSize) // Заполняем массив всех пикселей из ранговых блоков  
        {
            for (int i = 0; i < rankSize; i++)
            {
                for (int j = 0; j < rankSize; j++)
                {
                    pixels[(rankX * rankSize) + i, (rankY * rankSize) + j] = rank[i, j];
                }
            }
        }
    }
}
