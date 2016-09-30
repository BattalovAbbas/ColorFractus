using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourFractus
{
    [Serializable]
    public struct ImageDescription
    {
        public int width;
        public int height;
        public int rankSize;
        public List<Domain> listSaveDomain;
        public RankInfo[,] rankInfoRed;
        public RankInfo[,] rankInfoGreen;
        public RankInfo[,] rankInfoBlue;
    }
    [Serializable]
    public struct RankInfo  // Свойства рангового блока
    {
        public Domain domain;
        public int color;
    }
    [Serializable]
    public struct Domain  // Свойства рангового блока
    {
        public int domainX;
        public int domainY;
        public int rotType; // поворот преобразование (число от 0 до 7)       
        public Domain(int x, int y, int rotType)
        {
            domainX = x;
            domainY = y;
            this.rotType = rotType;
        }
    }
    public struct RankList
    {
        public List<int[,]> Red;
        public List<int[,]> Green;
        public List<int[,]> Blue;
        public List<int> sumRed;
        public List<int> sumGreen;
        public List<int> sumBlue;
    }
    public struct DomainList
    {
        public List<List<int[,]>> Red;
        public List<List<int[,]>> Green;
        public List<List<int[,]>> Blue;
        public List<int> sumRed;
        public List<int> sumGreen;
        public List<int> sumBlue;
    }
}
