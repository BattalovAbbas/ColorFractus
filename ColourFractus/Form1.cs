using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColourFractus
{
    public partial class ColorFractus : Form
    {
        public ColorFractus()
        {
            InitializeComponent();
        }
        private void btnCompress_Click(object sender, EventArgs e)
        {
            FileDescription file;
            int rankSize = int.Parse(comboBox1.Text);
            Stopwatch sw = new Stopwatch();
            FileInfo fileInfoInput;
            OpenFileDialog OPF = new OpenFileDialog();
            OPF.Filter = "Файлы bmp|*.bmp";
            if (OPF.ShowDialog() == DialogResult.OK)
            {
                string pathImage = OPF.FileName;
                fileInfoInput = new FileInfo(pathImage);
                file = new FileDescription(fileInfoInput.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(pathImage));
                sw.Start();
                Compress compress = new Compress(new Bitmap(pathImage), rankSize, 0.75, int.Parse(textBoxError.Text));
                ImageDescription imageDescription = compress.CompressStart();
                file.WriteFile(imageDescription, "imageDescription.bin");
                sw.Stop();
                textBoxTime.Text = "Изображение " + fileInfoInput.Name + " сжато";
                textBoxTime.Text += "\r\nВремя выполнения сжатия: " + Math.Round(sw.Elapsed.TotalSeconds, 3) + " секунд";
                textBoxTime.Text += "\r\nКоэффициент сжатия =" + Math.Round((double)fileInfoInput.Length / 1024 / file.size,3);
                //textBoxTime.Text += "\r\n" + Math.Round((double)file.count / (imageDescription.rankInfoBlue.Length * 3), 3);
            }
        }

        private void btnDecompress_Click(object sender, EventArgs e)
        {
            FileDescription file;
            int factor = (checkBox1.Checked) ? 2 : 1;
            Stopwatch sw = new Stopwatch();
            FileInfo fileInfoOutput;
            OpenFileDialog OPF = new OpenFileDialog();
            OPF.Filter = "Файлы fract|*.fract";
            if (OPF.ShowDialog() == DialogResult.OK)
            {
                string pathImage = OPF.FileName;
                fileInfoOutput = new FileInfo(pathImage);
                sw.Start();                
                file = new FileDescription(fileInfoOutput.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(pathImage));
                ImageDescription image = file.ReadFile("imageDescription.bin");
                Decompress decompress = new Decompress(fileInfoOutput.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(pathImage), image.rankSize, trackBar1.Value, 0.75, factor);
                decompress.imageDescription = image;             
                decompress.DecompressStart();
                sw.Stop();
                textBoxInfo.Text = "Время восстановления: " + Math.Round(sw.Elapsed.TotalSeconds,3) + " секунд";
            }
        }
    }
}
