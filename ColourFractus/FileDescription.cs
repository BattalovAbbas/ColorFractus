using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourFractus
{
    public class FileDescription
    {
        string pathImage;
        public int count = 0;
        public FileDescription(string pathImage)
        {
            this.pathImage = pathImage;
        }
        Random r = new Random();
        public long size;
        public void WriteFile(ImageDescription imageDescription, string path) // Запись данных в файл 
        {
            int width = imageDescription.rankInfoRed.GetLength(0);
            int height = imageDescription.rankInfoRed.GetLength(1);
            int position;
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                UnicodeEncoding uniEncoding = new UnicodeEncoding();
               
                using (BinaryWriter binWriter = new BinaryWriter(fileStream, Encoding.UTF8))
                {
                    binWriter.Write((Int16)imageDescription.width);                   
                    binWriter.Write((Int16)imageDescription.height);
                    binWriter.Write((byte)imageDescription.rankSize);
                    binWriter.Write((byte)imageDescription.listSaveDomain.Count);
                    for (int i = 0; i < imageDescription.listSaveDomain.Count; i++)
                    {
                        position = 8 + i;
                        binWriter.Write((byte)position);
                        binWriter.Write((Int16)imageDescription.listSaveDomain[i].domainX);
                        binWriter.Write((Int16)imageDescription.listSaveDomain[i].domainY);
                        binWriter.Write((byte)imageDescription.listSaveDomain[i].rotType);
                    }
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            binWriter.Write((sbyte)(imageDescription.rankInfoRed[i, j].color / 2));
                            position = imageDescription.listSaveDomain.IndexOf(imageDescription.rankInfoRed[i, j].domain);
                            if (position > -1)
                            {
                                position = position + 8;
                                binWriter.Write((byte)position);
                            }                            
                            else
                            {
                                binWriter.Write((byte)imageDescription.rankInfoRed[i, j].domain.rotType);
                                binWriter.Write((Int16)imageDescription.rankInfoRed[i, j].domain.domainX);
                                binWriter.Write((Int16)imageDescription.rankInfoRed[i, j].domain.domainY);
                            }

                            binWriter.Write((sbyte)(imageDescription.rankInfoGreen[i, j].color / 2));
                            position = imageDescription.listSaveDomain.IndexOf(imageDescription.rankInfoGreen[i, j].domain);
                            if (position > -1)
                            {
                                position = position + 8;
                                binWriter.Write((byte)position);
                            } 
                            else
                            {
                                binWriter.Write((byte)imageDescription.rankInfoGreen[i, j].domain.rotType);
                                binWriter.Write((Int16)imageDescription.rankInfoGreen[i, j].domain.domainX);
                                binWriter.Write((Int16)imageDescription.rankInfoGreen[i, j].domain.domainY);
                            }

                            binWriter.Write((sbyte)(imageDescription.rankInfoBlue[i, j].color / 2));
                            position = imageDescription.listSaveDomain.IndexOf(imageDescription.rankInfoBlue[i, j].domain);
                            if (position > -1)
                            {
                                position = position + 8;
                                binWriter.Write((byte)position);
                            } 
                            else
                            {
                                binWriter.Write((byte)imageDescription.rankInfoBlue[i, j].domain.rotType);
                                binWriter.Write((Int16)imageDescription.rankInfoBlue[i, j].domain.domainX);
                                binWriter.Write((Int16)imageDescription.rankInfoBlue[i, j].domain.domainY);
                            }
                        }
                    }
                }
            }
            // Архивация файла 
            using (FileStream fsInput = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (FileStream fsOutput = new FileStream(pathImage + ".fract", FileMode.Create, FileAccess.Write))
                {
                    using (GZipStream gzipStream = new GZipStream(fsOutput, CompressionMode.Compress))
                    {
                        Byte[] buffer = new Byte[fsInput.Length];
                        int h;
                        while ((h = fsInput.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            gzipStream.Write(buffer, 0, h);                            
                        }
                    }
                    
                }
            }
            FileInfo fileInfoInput = new FileInfo(pathImage + ".fract");
            size = fileInfoInput.Length/1024;
            File.Delete(path);
        }       
        public ImageDescription ReadFile(string path) // Чтение данных из файла
        {
            using (FileStream fsInput = new FileStream(pathImage + ".fract", FileMode.Open, FileAccess.Read))
            {
                using (FileStream fsOutput = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    using (GZipStream gzipStream = new GZipStream(fsInput, CompressionMode.Decompress))
                    {
                        Byte[] buffer = new Byte[fsInput.Length];
                        int h;
                        while ((h = gzipStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fsOutput.Write(buffer, 0, h);
                        }
                    }
                }
            }
            ImageDescription NewImage = new ImageDescription();
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                UnicodeEncoding uniEncoding = new UnicodeEncoding();
                using (BinaryReader binReader = new BinaryReader(fileStream, Encoding.UTF8))
                {
                    NewImage.width = (int)binReader.ReadInt16();
                    NewImage.height = (int)binReader.ReadInt16();
                    NewImage.rankSize = (int)binReader.ReadByte();
                    int count = (int)binReader.ReadByte();
                    Dictionary<int, Domain> DictionarySaveDomain = new Dictionary<int, Domain>();
                    for (int i = 0; i < count; i++)
                    {
                        int a = (int)binReader.ReadByte();
                        Domain d = new Domain((int)binReader.ReadInt16(), (int)binReader.ReadInt16(), (int)binReader.ReadByte());
                        DictionarySaveDomain.Add(a, d);
                    }
                    NewImage.rankInfoRed = new RankInfo[NewImage.width / NewImage.rankSize, NewImage.height / NewImage.rankSize];
                    NewImage.rankInfoGreen = new RankInfo[NewImage.width / NewImage.rankSize, NewImage.height / NewImage.rankSize];
                    NewImage.rankInfoBlue = new RankInfo[NewImage.width / NewImage.rankSize, NewImage.height / NewImage.rankSize];
                    int width = NewImage.width / NewImage.rankSize;
                    int height = NewImage.height / NewImage.rankSize;
                    int info;
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            NewImage.rankInfoRed[i, j].color = (int)binReader.ReadSByte() * 2;
                            info = (int)binReader.ReadByte();
                            if (DictionarySaveDomain.ContainsKey(info))
                            {
                                NewImage.rankInfoRed[i, j].domain = DictionarySaveDomain[info];
                            }
                            else
                            {
                                NewImage.rankInfoRed[i, j].domain = new Domain((int)binReader.ReadInt16(), (int)binReader.ReadInt16(), info);
                            }
                            NewImage.rankInfoGreen[i, j].color = (int)binReader.ReadSByte() * 2;
                            info = (int)binReader.ReadByte();                            
                            if (DictionarySaveDomain.ContainsKey(info))
                            {
                                NewImage.rankInfoGreen[i, j].domain = DictionarySaveDomain[info];
                            }
                            else
                            {
                                NewImage.rankInfoGreen[i, j].domain = new Domain((int)binReader.ReadInt16(), (int)binReader.ReadInt16(), info);
                            }
                            NewImage.rankInfoBlue[i, j].color = (int)binReader.ReadSByte() * 2;
                            info = (int)binReader.ReadByte();                            
                            if (DictionarySaveDomain.ContainsKey(info))
                            {
                                NewImage.rankInfoBlue[i, j].domain = DictionarySaveDomain[info];
                            }
                            else
                            {
                                NewImage.rankInfoBlue[i, j].domain = new Domain((int)binReader.ReadInt16(), (int)binReader.ReadInt16(), info);
                            }
                        }
                    }
                }
            }
            File.Delete(path);
            return NewImage;
        }
    }
}
